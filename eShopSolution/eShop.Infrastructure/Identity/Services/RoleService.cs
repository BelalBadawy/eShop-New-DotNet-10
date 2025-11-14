using eShop.Application.Features.Roles;
using eShop.Application.Features.Roles.Commands;
using eShop.Application.Models;
using eShop.Infrastructure.Identity.Constants;
using eShop.Infrastructure.Identity.Models;
using eShop.Infrastructure.Persistence.Contexts;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using eShop.Application.Features.Roles.Queries;

namespace eShop.Infrastructure.Identity.Services
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public RoleService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IResponseWrapper> CreateRoleAsync(CreateRoleRequest createRole)
        {
            var roleInDb = await _roleManager.FindByNameAsync(createRole.Name);
            if (roleInDb is not null)
            {
                return await ResponseWrapper.FailAsync("Role already exists");
            }

            var newRole = new ApplicationRole
            {
                Name = createRole.Name,
                Description = createRole.Description
            };

            var identityResult = await _roleManager.CreateAsync(newRole);
            if (identityResult.Succeeded)
            {
                return await ResponseWrapper.SuccessAsync(message: "Role created successfully");
            }
            return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityResult));
        }

        public async Task<IResponseWrapper> DeleteRoleAsync(int roleId)
        {

            if (roleId == 0)
            {
                return await ResponseWrapper.FailAsync("Role Id is required.");
            }

            var roleInDb = await _roleManager.FindByIdAsync(roleId.ToString());
            if (roleInDb is not null)
            {
                if (roleInDb.Name != AppRoles.Admin)
                {

                    var usersInRole = await _userManager.GetUsersInRoleAsync(roleInDb.Name);

                    if (usersInRole.Any())
                    {
                        return await ResponseWrapper
                            .FailAsync($"Role: {roleInDb.Name} is currently assigned to a user.");
                    }

                    var identityResult = await _roleManager.DeleteAsync(roleInDb);
                    if (identityResult.Succeeded)
                    {
                        return await ResponseWrapper.SuccessAsync("Role successfully deleted.");
                    }
                    return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityResult));
                }
                return await ResponseWrapper.FailAsync("Cannot delete Admin role.");
            }
            return await ResponseWrapper.FailAsync("Role does not exist.");
        }

        public async Task<IResponseWrapper> GetPermissionsAsync(int roleId)
        {
            var roleInDb = await _roleManager.FindByIdAsync(roleId.ToString());
            if (roleInDb is not null)
            {
                var allPermissions = AppPermissions.AllPermissions;
                var roleClaimResponse = new RoleClaimResponse
                {
                    Role = new RoleResponse
                    {
                        Id = roleId,
                        Name = roleInDb.Name,
                        Description = roleInDb.Description
                    },
                    RoleClaims = new List<RoleClaimViewModel>()
                };

                var currentlyAssignedClaims = await GetAllClaimsForRoleAsync(roleId);

                var allPermissionNames = allPermissions.Select(p => p.Name).ToList(); // Permission.Identity.Users.Create

                var currentlyAssignedClaimsValues = currentlyAssignedClaims
                    .Select(rc => rc.ClaimValue).ToList();// Permission.Identity.Users.Create

                var currentlyAssignedRoleClaimsNames = allPermissionNames
                    .Intersect(currentlyAssignedClaimsValues)
                    .ToList();

                foreach (var permission in allPermissions)
                {

                    roleClaimResponse.RoleClaims.Add(new RoleClaimViewModel
                    {
                        ClaimType = AppClaim.Permission,
                        ClaimValue = permission.Name,
                        Description = permission.Description,
                    });
                }

                return await ResponseWrapper<RoleClaimResponse>.SuccessAsync(data: roleClaimResponse);
            }
            return await ResponseWrapper.FailAsync(message: "Role does not exist.");
        }

        public async Task<IResponseWrapper> GetRoleByIdAsync(int roleId)
        {
            var roleInDb = await _roleManager.FindByIdAsync(roleId.ToString());
            if (roleInDb is not null)
            {
                var mappedRole = roleInDb.Adapt<RoleResponse>();
                return await ResponseWrapper<RoleResponse>.SuccessAsync(data: mappedRole);
            }
            return await ResponseWrapper.FailAsync("Role does not exist.");
        }

        public async Task<IResponseWrapper> GetRolesAsync()
        {
            var allRoles = await _roleManager.Roles.ToListAsync();
            if (allRoles.Count > 0)
            {
                var mappedRoles = allRoles.Adapt<List<RoleResponse>>();
                return await ResponseWrapper<List<RoleResponse>>.SuccessAsync(data: mappedRoles);
            }
            return await ResponseWrapper.FailAsync("No roles were found.");
        }

        public async Task<IResponseWrapper> UpdateRoleAsync(UpdateRoleRequest updateRole)
        {
            var roleInDb = await _roleManager.FindByIdAsync(updateRole.RoleId.ToString());
            if (roleInDb is not null)
            {
                if (roleInDb.Name != AppRoles.Admin)
                {
                    roleInDb.Name = updateRole.Name;
                    roleInDb.Description = updateRole.Description;

                    var identityResult = await _roleManager.UpdateAsync(roleInDb);
                    if (identityResult.Succeeded)
                    {
                        return await ResponseWrapper.SuccessAsync("Role updated successfully");
                    }
                    return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityResult));

                }
                return await ResponseWrapper.FailAsync("Cannot update Admin role.");
            }
            return await ResponseWrapper.FailAsync("Role does not exist.");
        }

        public async Task<IResponseWrapper> UpdateRolePermissionsAsync(UpdateRoleClaimsRequest updateRoleClaims)
        {
            var roleInDb = await _roleManager.FindByIdAsync(updateRoleClaims.RoleId.ToString());
            if (roleInDb is null)
                return await ResponseWrapper.FailAsync("Role does not exist.");

            if (roleInDb.Name == AppRoles.Admin)
                return await ResponseWrapper.FailAsync("Cannot change permissions for this role.");

            var existingClaims = await _roleManager.GetClaimsAsync(roleInDb);
            var newClaims = updateRoleClaims.RoleClaims
                .Select(rc => new Claim(rc.ClaimType, rc.ClaimValue))
                .ToList();

            var claimsToAdd = newClaims
                .Where(nc => !existingClaims.Any(ec => ec.Type == nc.Type && ec.Value == nc.Value))
                .ToList();

            var claimsToRemove = existingClaims
                .Where(ec => !newClaims.Any(nc => nc.Type == ec.Type && nc.Value == ec.Value))
                .ToList();

            if (!claimsToAdd.Any() && !claimsToRemove.Any())
                return await ResponseWrapper.SuccessAsync("No changes detected.");

            // Use execution strategy
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var claim in claimsToRemove)
                        await _roleManager.RemoveClaimAsync(roleInDb, claim);

                    foreach (var claim in claimsToAdd)
                        await _roleManager.AddClaimAsync(roleInDb, claim);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw; // Important for retry
                }
            });

            return await ResponseWrapper.SuccessAsync("Role permissions updated successfully.");
        }



        #region Private Helpers
        private List<string> GetIdentityResultErrorDescriptions(IdentityResult identityResult)
        {
            var errorDescriptions = new List<string>();
            foreach (var error in identityResult.Errors)
            {
                errorDescriptions.Add(error.Description);
            }
            return errorDescriptions;
        }

        private async Task<List<RoleClaimViewModel>> GetAllClaimsForRoleAsync(int roleId)
        {
            var roleClaims = await _context.RoleClaims
                .Where(rc => rc.RoleId == roleId)
                .ToListAsync();

            if (roleClaims.Count > 0)
            {
                var mappedRoleClaims = roleClaims.Adapt<List<RoleClaimViewModel>>();
                return mappedRoleClaims;
            }
            return [];
        }
        #endregion
    }
}

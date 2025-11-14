using eShop.Application.Features.Users;
using eShop.Application.Features.Users.Commands;
using eShop.Application.Features.Users.Models.Requests;
using eShop.Application.Features.Users.Models.Responses;
using eShop.Application.Helpers;
using eShop.Application.Interfaces;
using eShop.Application.Models;
using eShop.Application.Models.Common;
using eShop.Application.Models.Pagination;
using eShop.Infrastructure.Identity.Constants;
using eShop.Infrastructure.Identity.Models;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace eShop.Infrastructure.Identity.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IEmailService emailService,
            IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _httpContextAccessor = contextAccessor;
        }

        public async Task<IResponseWrapper> RegisterUserAsync(UserRegistrationRequest userRegistration)
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(userRegistration.Email);
            if (userWithSameEmail is not null)
                return await ResponseWrapper.FailAsync("Email address already taken.");

            var newUser = new ApplicationUser
            {
                FullName = userRegistration.FullName,
                Email = userRegistration.Email,
                UserName = userRegistration.Email,
                PhoneNumber = userRegistration.PhoneNumber,
                IsActive = userRegistration.ActivateUser,
                EmailConfirmed = userRegistration.AutoConfirmEmail,
                RefreshToken = DateTime.Now.Ticks.ToString(),
                RefreshTokenExpiryDate = DateTime.Now.AddDays(1)
            };

            // Password Hash
            var password = new PasswordHasher<ApplicationUser>();

            newUser.PasswordHash = password.HashPassword(newUser, userRegistration.Password);

            var identityUserResult = await _userManager.CreateAsync(newUser);

            if (identityUserResult.Succeeded)
            {
                var identityRoleResult = await _userManager.AddToRoleAsync(newUser, AppRoles.Basic);

                if (identityRoleResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync("User registered successfully.");
                }
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityRoleResult));
            }
            return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityUserResult));
        }

        public async Task<IResponseWrapper> UpdateUserAsync(UpdateUserRequest userUpdate)
        {
            var userInDb = await _userManager.FindByIdAsync(userUpdate.UserId.ToString());
            if (userInDb is not null)
            {
                userInDb.FullName = userUpdate.FullName;
                userInDb.PhoneNumber = userUpdate.PhoneNumber;

                var identityResult = await _userManager.UpdateAsync(userInDb);
                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync("User updated successfully.");
                }
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityResult));
            }
            return await ResponseWrapper.FailAsync("User does not exists.");
        }

        #region Private Helpers
        private static List<string> GetIdentityResultErrorDescriptions(IdentityResult identityResult)
        {
            var errorDescriptions = new List<string>();
            foreach (var error in identityResult.Errors)
            {
                errorDescriptions.Add(error.Description);
            }
            return errorDescriptions;
        }
        #endregion

        public async Task<IResponseWrapper> GetUserByIdAsync(int userId, CancellationToken ct)
        {
            var userInDb = await _userManager.FindByIdAsync(userId.ToString());
            if (userInDb is not null)
            {
                var mappedUser = userInDb.Adapt<UserResponse>();
                return await ResponseWrapper<UserResponse>.SuccessAsync(data: mappedUser);
            }
            return await ResponseWrapper.FailAsync("User does not exists.");
        }

        public async Task<IResponseWrapper> GetAllUsersAsync(CancellationToken ct)
        {
            var usersInDb = await _userManager
                .Users
                .ToListAsync();

            if (usersInDb.Count > 0)
            {
                var mappedUsers = usersInDb.Adapt<List<UserResponse>>();

                return await ResponseWrapper<List<UserResponse>>.SuccessAsync(data: mappedUsers);
            }
            return await ResponseWrapper.FailAsync("No Users were found.");
        }


        public async Task<IResponseWrapper> GetUsersPagedQueryAsync(PagedFilterRequest pagedFilterRequest, CancellationToken ct)
        {

            var usersQuery = _userManager.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(pagedFilterRequest.SearchTerm))
            {
                var term = pagedFilterRequest.SearchTerm.ToLower();

                usersQuery = usersQuery.Where(u =>
                    u.FullName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term)
                );
            }

            // Sorting — no helper method
            usersQuery = pagedFilterRequest.SortBy?.ToLower() switch
            {
                "email" => pagedFilterRequest.SortDirection == "desc"
                    ? usersQuery.OrderByDescending(u => u.Email)
                    : usersQuery.OrderBy(u => u.Email),

                "id" => pagedFilterRequest.SortDirection == "desc"
                    ? usersQuery.OrderByDescending(u => u.Id)
                    : usersQuery.OrderBy(u => u.Id),

                "fullname" or _ => pagedFilterRequest.SortDirection == "desc"
                    ? usersQuery.OrderByDescending(u => u.FullName)
                    : usersQuery.OrderBy(u => u.FullName),
            };

            // Pagination
            var totalRecords = await usersQuery.CountAsync(ct);

            var users = await usersQuery
                .Skip((pagedFilterRequest.PageNumber - 1) * pagedFilterRequest.PageSize)
                .Take(pagedFilterRequest.PageSize)
                .Select(o => new UserResponse()
                {
                    FullName = o.FullName,
                    Email = o.Email,
                    Id = o.Id,
                    IsActive = o.IsActive,
                    PhoneNumber = o.PhoneNumber,
                    UserName = o.UserName,
                    EmailConfirmed = o.EmailConfirmed
                })
                .ToListAsync(ct);

            var data = new PagedResult<UserResponse>
            {
                Data = users,
                TotalCount = totalRecords,
                CurrentPage = pagedFilterRequest.PageNumber,
                PageSize = pagedFilterRequest.PageSize,

            };

            return await ResponseWrapper<PagedResult<UserResponse>>.SuccessAsync(data: data);

        }


        public async Task<IResponseWrapper> ChangeUserPasswordAsync(ChangePasswordRequest changePassword, CancellationToken ct)
        {
            var userInDb = await _userManager.FindByIdAsync(changePassword.UserId.ToString());
            if (userInDb is not null)
            {
                var identityResult = await _userManager.ChangePasswordAsync(
                userInDb,
                    changePassword.CurrentPassword,
                    changePassword.NewPassword);

                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper.SuccessAsync(message: "User password updated.");
                }
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityResult));
            }
            return await ResponseWrapper.FailAsync("User does not exist.");
        }

        public async Task<IResponseWrapper> ChangeUserStatusAsync(ChangeUserStatusRequest changeUserStatus, CancellationToken ct)
        {
            var userInDb = await _userManager.FindByIdAsync(changeUserStatus.UserId.ToString());
            if (userInDb is not null)
            {
                // Change status
                userInDb.IsActive = changeUserStatus.ActivateOrDeactivate;

                var identityResult = await _userManager.UpdateAsync(userInDb);

                if (identityResult.Succeeded)
                {
                    return await ResponseWrapper
                        .SuccessAsync(changeUserStatus.ActivateOrDeactivate ? "User activated successfully."
                            : "User de-activated successfully");
                }
                return await ResponseWrapper
                    .FailAsync(GetIdentityResultErrorDescriptions(identityResult));
            }
            return await ResponseWrapper.FailAsync("User does not exist.");
        }

        public async Task<IResponseWrapper> GetUserRolesAsync(int userId, CancellationToken ct)
        {
            var userRolesViewModel = new List<UserRoleViewModel>();
            var userInDb = await _userManager.FindByIdAsync(userId.ToString());

            if (userInDb is not null)
            {
                var allRoles = await _roleManager.Roles.ToListAsync();

                foreach (var role in allRoles)
                {
                    var userRoleViewModel = new UserRoleViewModel
                    {
                        RoleName = role.Name,
                        RoleDescription = role.Description
                    };

                    if (await _userManager.IsInRoleAsync(userInDb, role.Name))
                    {
                        //userRoleViewModel.IsAssignedToUser = true;
                        userRolesViewModel.Add(userRoleViewModel);
                    }
                    //else
                    //{
                    //    userRoleViewModel.IsAssignedToUser = false;
                    //}
                }

                return await ResponseWrapper<List<UserRoleViewModel>>.SuccessAsync(userRolesViewModel);
            }
            return await ResponseWrapper.FailAsync("User does not exist.");
        }

        //public async Task<IResponseWrapper> UpdateUserRolesAsync(UpdateUserRolesRequest updateUserRoles, CancellationToken ct)
        //{
        //    var userInDb = await _userManager.FindByIdAsync(updateUserRoles.UserId);
        //    if (userInDb is not null)
        //    {
        //        if (userInDb.Email == AppCredentials.Email)
        //        {
        //            return await ResponseWrapper.FailAsync(message: "User Roles update not permitted.");
        //        }

        //        var currentAssigneRoles = await _userManager.GetRolesAsync(userInDb);

        //        var rolesToBeAssigned = updateUserRoles.Roles
        //            .Where(role => role.IsAssignedToUser == true)
        //            .ToList();

        //        var identityRemovingResult = await _userManager.RemoveFromRolesAsync(userInDb, currentAssigneRoles);

        //        if (identityRemovingResult.Succeeded)
        //        {
        //            var identityAssigningResult = await _userManager
        //                .AddToRolesAsync(userInDb, rolesToBeAssigned.Select(role => role.RoleName));

        //            if (identityAssigningResult.Succeeded)
        //            {
        //                return await ResponseWrapper.SuccessAsync(message: "Updated user roles successfully.");
        //            }
        //            return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityAssigningResult));
        //        }
        //        return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(identityRemovingResult));
        //    }
        //    return await ResponseWrapper.FailAsync("User does not exist.");
        //}

        public async Task<IResponseWrapper> UpdateUserRolesAsync(UpdateUserRolesRequest request, CancellationToken ct)
        {
            // Find user (supports CancellationToken via EF query)
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, ct);

            if (user is null)
                return await ResponseWrapper.FailAsync("User does not exist.");

            // Optional system protection
            if (user.Email == AppCredentials.Email)
                return await ResponseWrapper.FailAsync("User roles update not permitted.");

            // Roles to assign (all roles sent are to be assigned)
            var rolesToAssign = request.Roles.ToList();

            // Validate each role exists
            foreach (var roleName in rolesToAssign)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    return await ResponseWrapper.FailAsync($"Role '{roleName}' does not exist.");
            }

            // Get current roles
            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove all roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(removeResult));

            // Assign new roles
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
            if (!addResult.Succeeded)
                return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(addResult));

            return await ResponseWrapper.SuccessAsync("Updated user roles successfully.");
        }

        public async Task<IResponseWrapper> ForgotPasswordAsync(string email, CancellationToken ct)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return await ResponseWrapper.FailAsync("This email doesn't exist.");

            if (!user.EmailConfirmed)
                return await ResponseWrapper.FailAsync("This email is not confirmed.");

            // Build reset password URL
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{baseUrl}/Account/ResetPassword?email={user.Email}&code={code}";

            // Prepare email
            var emailModel = new SendEmailDto
            {
                Subject = "Reset Password",
                MailTo = user.Email,
                //MessageBody = $"Please reset your password by clicking here: <a href=\"{callbackUrl}\">link</a>"
                MessageBody = $"<p>Hello: {user.FullName}</p>" +
                $"<p>Username: {user.UserName}.</p>" +
                "<p>In order to reset your password, please click on the following link.</p>" +
                $"<p><a href=\"{callbackUrl}\">Click here</a></p>" +
                "<p>Thank you,</p>"
            };

            try
            {
                var result = await _emailService.SendAsync(emailModel);

                if (string.IsNullOrEmpty(result))
                    return await ResponseWrapper.SuccessAsync("Reset password email sent successfully.");

                return await ResponseWrapper.FailAsync(result);
            }
            catch (Exception ex)
            {
                return await ResponseWrapper.FailAsync(ex.Message);
            }
        }

        public async Task<IResponseWrapper> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return await ResponseWrapper.FailAsync("This email doesn't exist.");

            if (!user.EmailConfirmed)
                return await ResponseWrapper.FailAsync("This email is not confirmed.");

            try
            {
                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);

                if (result.Succeeded)
                {
                    //That immediately invalidates all existing tokens for that user.
                    await _userManager.UpdateSecurityStampAsync(user);

                    return await ResponseWrapper.SuccessAsync("Your password has changed successfully.");
                }
                else
                {
                    return await ResponseWrapper.FailAsync(GetIdentityResultErrorDescriptions(result));
                }

            }
            catch (Exception ex)
            {
                return await ResponseWrapper.FailAsync(SD.ErrorOccured);
            }
        }
    }
}

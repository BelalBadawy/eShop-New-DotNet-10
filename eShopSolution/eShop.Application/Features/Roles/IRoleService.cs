using eShop.Application.Features.Roles.Commands;

namespace eShop.Application.Features.Roles
{
    public interface IRoleService
    {
        Task<IResponseWrapper> CreateRoleAsync(CreateRoleRequest createRole);
        Task<IResponseWrapper> GetRolesAsync();
        Task<IResponseWrapper> UpdateRoleAsync(UpdateRoleRequest updateRole);
        Task<IResponseWrapper> GetRoleByIdAsync(int roleId);
        Task<IResponseWrapper> DeleteRoleAsync(int roleId);
        Task<IResponseWrapper> GetPermissionsAsync(int roleId);
        Task<IResponseWrapper> UpdateRolePermissionsAsync(UpdateRoleClaimsRequest updateRoleClaims);
    }
}

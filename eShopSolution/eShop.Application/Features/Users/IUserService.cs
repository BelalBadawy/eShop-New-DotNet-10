using eShop.Application.Features.Users.Commands;
using eShop.Application.Models.Pagination;

namespace eShop.Application.Features.Users
{
    public interface IUserService
    {
        Task<IResponseWrapper> RegisterUserAsync(UserRegistrationRequest userRegistration);
        Task<IResponseWrapper> UpdateUserAsync(UpdateUserRequest userUpdate);

        // Start
        Task<IResponseWrapper> GetUserByIdAsync(int userId, CancellationToken ct);
        Task<IResponseWrapper> GetAllUsersAsync(CancellationToken ct);
        Task<IResponseWrapper> GetUsersPagedQueryAsync(PagedFilterRequest pagedFilterRequest, CancellationToken ct);
        Task<IResponseWrapper> ChangeUserPasswordAsync(ChangePasswordRequest changePassword, CancellationToken ct);
        Task<IResponseWrapper> ChangeUserStatusAsync(ChangeUserStatusRequest changeUserStatus, CancellationToken ct);
        Task<IResponseWrapper> GetUserRolesAsync(int userId, CancellationToken ct);
        Task<IResponseWrapper> UpdateUserRolesAsync(UpdateUserRolesRequest updateUserRoles, CancellationToken ct);
        Task<IResponseWrapper> ForgotPasswordAsync(string email, CancellationToken ct);
        Task<IResponseWrapper> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct);

        // End
    }
}

using eShop.Application.Features.Menus.Commands;
using eShop.Application.Features.Users.Commands;

namespace eShop.Application.Features.Menus
{
    public interface IMenuService
    {
        
        Task<IResponseWrapper> CreateMenusAsync(CreateMenuRequest request,CancellationToken ct);
        Task<IResponseWrapper> UpdateMenusAsync(UpdateMenuRequest request,CancellationToken ct);
        Task<IResponseWrapper> DeleteMenuAsync(int menuId, CancellationToken ct);
        Task<IResponseWrapper> GetMenuByIdAsync(int menuId, CancellationToken ct);
        Task<IResponseWrapper> GetAllMenusAsync(string type,CancellationToken ct);
        Task<IResponseWrapper> GetAllMenusForListAsync(string type, CancellationToken ct);
    }
}

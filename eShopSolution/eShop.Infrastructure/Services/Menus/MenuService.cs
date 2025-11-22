using eShop.Application.Features.Menus;
using eShop.Application.Features.Menus.Commands;
using eShop.Application.Features.Menus.Queries;
using eShop.Application.Features.Menus.Queries.GetAllMenusForList;
using eShop.Application.Models;
using eShop.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;

namespace eShop.Infrastructure.Services.Menus
{
    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _dbContext;

        public MenuService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IResponseWrapper> CreateMenusAsync(CreateMenuRequest request, CancellationToken ct)
        {
            // Check if a menu with same Title + Type exists
            var exists = await _dbContext.Menus
                                    .AsNoTracking()
                                    .AnyAsync(x =>
                                        EF.Functions.Like(x.Title.Trim(), request.Title.Trim()) &&
                                        EF.Functions.Like(x.Type.Trim(), request.Type.Trim()),
                                        ct);

            if (exists)
            {
                return await ResponseWrapper.FailAsync("Menu item already exist with this type.");
            }

            // Map to entity
            var menu = new Menu
            {
                Title = request.Title,
                Link = request.Link,
                Type = request.Type,
                ParentId = request.ParentId,
                Order = request.Order
            };

            await _dbContext.Menus.AddAsync(menu, ct);
            await _dbContext.SaveChangesAsync(ct);

            return await ResponseWrapper.SuccessAsync("Menu item added successfully.");
        }

        public async Task<IResponseWrapper> DeleteMenuAsync(int menuId, CancellationToken ct)
        {
            if (menuId == 0)
            {
                return await ResponseWrapper.FailAsync("Menu Id is required.");
            }

            if(!await _dbContext.Menus.AnyAsync(o => o.Id == menuId,ct))
            {
                return await ResponseWrapper.FailAsync("Menu item not found.");
            }

            await _dbContext.Menus.Where(o=>o.Id == menuId).ExecuteDeleteAsync(ct);
                      
           return await ResponseWrapper.SuccessAsync("Menu item successfully deleted.");
        }

        public async Task<IResponseWrapper> GetAllMenusAsync(string type, CancellationToken ct)
        {
            var menus = await _dbContext.Menus
                                       .AsNoTracking()
                                       .Where(o => EF.Functions.Like(o.Type, type))
                                       .OrderBy(o => o.Order)
                                       .Select(o=> new MenuResponse()
                                       {
                                             Id = o.Id,
                                             Title = o.Title,
                                             Link = o.Link,
                                             Type = o.Type,
                                             ParentId = o.ParentId,
                                             Order = o.Order
                                       })
                                       .ToListAsync(ct);

            return await ResponseWrapper<List<MenuResponse>>.SuccessAsync(data: menus);
        }

        public async Task<IResponseWrapper> GetAllMenusForListAsync(string type, CancellationToken ct)
        {
            var menus = await _dbContext.Menus
                                       .AsNoTracking()
                                       .Where(o => EF.Functions.Like(o.Type, type))
                                       .OrderBy(o => o.Order)
                                       .Select(o => new MenuLookupDto()
                                       {
                                           Id = o.Id,
                                           Title = o.Title,
                                       })
                                       .ToListAsync(ct);

            return await ResponseWrapper<List<MenuLookupDto>>.SuccessAsync(data: menus);
        }
        public async Task<IResponseWrapper> GetMenuByIdAsync(int menuId, CancellationToken ct)
        {
            var menu = await _dbContext.Menus
                                    .AsNoTracking()
                                    .Where(o => o.Id == menuId)
                                    .Select(o => new MenuResponse
                                    {
                                        Id = o.Id,
                                        Title = o.Title,
                                        Link = o.Link,
                                        Type = o.Type,
                                        ParentId = o.ParentId,
                                        Order = o.Order
                                    })
                                    .FirstOrDefaultAsync(ct);

            if(menu == null)
            {
                return await ResponseWrapper.FailAsync("Menu item not found.");
            }

            return await ResponseWrapper<MenuResponse>.SuccessAsync(data: menu);
        }
        public async Task<IResponseWrapper> UpdateMenusAsync(UpdateMenuRequest request, CancellationToken ct)
        {
            // Check if a menu with same Title + Type exists (excluding current menu)
            var exists = await _dbContext.Menus
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != request.Id &&
                    x.Title.Trim().ToLower() == request.Title.Trim().ToLower() &&
                    x.Type.Trim().ToLower() == request.Type.Trim().ToLower(),
                    ct);

            if (exists)
            {
                return await ResponseWrapper.FailAsync("Menu item already exist with this type.");
            }

            // Fetch existing menu
            var menu = await _dbContext.Menus.FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (menu == null)
            {
                return await ResponseWrapper.FailAsync("Menu item not found.");
            }

            // Update values
            menu.Title = request.Title;
            menu.Link = request.Link;
            menu.Type = request.Type;
            menu.ParentId = request.ParentId;
            menu.Order = request.Order;

            // Save changes
            await _dbContext.SaveChangesAsync(ct);

            return await ResponseWrapper.SuccessAsync("Menu item updated successfully.");
        }

    }
}

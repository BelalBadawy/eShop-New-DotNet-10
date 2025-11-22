using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Application.Features.Menus.Queries
{
    public class GetAllMenusForListByTypeQuery : IRequest<IResponseWrapper>
    {
        public string Type { get; set; } = string.Empty;
    }

    public class GetAllMenusForListByTypeQueryHandler : IRequestHandler<GetAllMenusForListByTypeQuery, IResponseWrapper>
    {
        private readonly IMenuService _menuService;
        public GetAllMenusForListByTypeQueryHandler(IMenuService menuService)
        {
            _menuService = menuService;
        }
        public async Task<IResponseWrapper> Handle(GetAllMenusForListByTypeQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Type))
            {
                return await ResponseWrapper.FailAsync("Menu type is required.");
            }

            return await _menuService.GetAllMenusForListAsync(request.Type, cancellationToken);
        }
    }
}

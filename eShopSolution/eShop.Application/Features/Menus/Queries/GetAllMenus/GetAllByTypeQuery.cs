using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Application.Features.Menus.Queries
{
    public class GetAllByTypeQuery : IRequest<IResponseWrapper>
    {
        public string Type { get; set; } = string.Empty;
    }

    public class GetAllByTypeQueryHandler : IRequestHandler<GetAllByTypeQuery, IResponseWrapper>
    {
        private readonly IMenuService _menuService;
        public GetAllByTypeQueryHandler(IMenuService menuService)
        {
            _menuService = menuService;
        }
        public async Task<IResponseWrapper> Handle(GetAllByTypeQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Type))
            {
                return await ResponseWrapper.FailAsync("Menu type is required.");
            }

            return await _menuService.GetAllMenusAsync(request.Type, cancellationToken);
        }
    }
}

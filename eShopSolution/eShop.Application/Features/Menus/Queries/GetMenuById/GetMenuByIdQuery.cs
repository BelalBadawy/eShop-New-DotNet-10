namespace eShop.Application.Features.Menus.Queries
{
    public class GetMenuByIdQuery : IRequest<IResponseWrapper>
    {
        public int MenuId { get; set; }
    }

    public class GetMenuByIdQueryHandler : IRequestHandler<GetMenuByIdQuery, IResponseWrapper>
    {
        private readonly IMenuService _menuService;

        public GetMenuByIdQueryHandler(IMenuService MenuService)
        {
            _menuService = MenuService;
        }

        public async Task<IResponseWrapper> Handle(GetMenuByIdQuery request, CancellationToken cancellationToken)
        {
            return await _menuService.GetMenuByIdAsync(request.MenuId, cancellationToken);
        }
    }
}

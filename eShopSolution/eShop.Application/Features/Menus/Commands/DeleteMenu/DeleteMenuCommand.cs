namespace eShop.Application.Features.Menus.Commands
{
    public class DeleteMenuCommand : IRequest<IResponseWrapper>
    {
        public int MenuId { get; set; }
    }

    public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, IResponseWrapper>
    {
        private readonly IMenuService _menuService;

        public DeleteMenuCommandHandler(IMenuService MenuService)
        {
            _menuService = MenuService;
        }

        public async Task<IResponseWrapper> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
        {
            return await _menuService.DeleteMenuAsync(request.MenuId, cancellationToken);
        }
    }
}

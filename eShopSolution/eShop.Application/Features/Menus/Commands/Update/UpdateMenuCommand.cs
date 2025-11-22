namespace eShop.Application.Features.Menus.Commands
{
    public class UpdateMenuCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public UpdateMenuRequest UpdateMenuRequest { get; set; }
    }

    public class UpdateMenuCommandCommandHandler(IMenuService menuService)
        : IRequestHandler<UpdateMenuCommand, IResponseWrapper>
    {
        private readonly IMenuService _menuService = menuService;

        public async Task<IResponseWrapper> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
        {
            return await _menuService.UpdateMenusAsync(request.UpdateMenuRequest, cancellationToken);
        }
    }
}

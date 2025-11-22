
using eShop.Application.Features.Menus;

namespace eShop.Application.Features.Menus.Commands
{
    public class CreateMenuCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public CreateMenuRequest CreateMenuRequest { get; set; }
    }

    public class CreateMenuCommandCommandHandler(IMenuService menuService)
        : IRequestHandler<CreateMenuCommand, IResponseWrapper>
    {
        private readonly IMenuService _menuService = menuService;

        public async Task<IResponseWrapper> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
        {
            return await _menuService.CreateMenusAsync(request.CreateMenuRequest, cancellationToken);
        }
    }
}

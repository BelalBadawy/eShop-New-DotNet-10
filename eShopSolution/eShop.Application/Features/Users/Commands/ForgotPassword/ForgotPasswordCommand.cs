namespace eShop.Application.Features.Users.Commands
{
    public class ForgotPasswordCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public string Email { get; set; }
    }

    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, IResponseWrapper>
    {
        private readonly IUserService _userService;

        public ForgotPasswordCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IResponseWrapper> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ForgotPasswordAsync(request.Email, cancellationToken);
        }
    }
}

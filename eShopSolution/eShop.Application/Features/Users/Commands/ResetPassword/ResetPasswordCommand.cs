namespace eShop.Application.Features.Users.Commands
{
    public class ResetPasswordCommand : IRequest<IResponseWrapper>, IValidateMe
    {
        public ResetPasswordRequest ResetPasswordRequest { get; set; }
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, IResponseWrapper>
    {
        private readonly IUserService _userService;

        public ResetPasswordCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IResponseWrapper> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ResetPasswordAsync(request.ResetPasswordRequest, cancellationToken);
        }
    }
}

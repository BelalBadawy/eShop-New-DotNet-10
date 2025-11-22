
namespace eShop.Application.Features.Menus.Commands
{
    public class CreateMenuCommandValidator : AbstractValidator<CreateMenuCommand>
    {
        public CreateMenuCommandValidator()
        {
            RuleFor(x => x.CreateMenuRequest)
           .NotNull()
           .WithMessage("Menu data is required.");

            RuleFor(x => x.CreateMenuRequest.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.CreateMenuRequest.Type)
                .NotEmpty()
                .WithMessage("Type is required.")
                .Must(type => type == "Top" || type == "Bottom")
                .WithMessage("Type must be either 'Top' or 'Bottom'.");

            RuleFor(x => x.CreateMenuRequest.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Order must be zero or a positive number.");

            RuleFor(x => x.CreateMenuRequest.Link)
                .NotNull()
                .WithMessage("Link is required.")
                .NotEmpty()
                .WithMessage("Link is required.")
                .MaximumLength(500)
                .WithMessage("Link cannot exceed 500 characters.");
        }
    }
}

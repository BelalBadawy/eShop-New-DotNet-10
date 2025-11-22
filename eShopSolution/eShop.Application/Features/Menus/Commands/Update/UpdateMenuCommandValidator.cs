
namespace eShop.Application.Features.Menus.Commands
{
    public class UpdateMenuCommandValidator : AbstractValidator<UpdateMenuCommand>
    {
        public UpdateMenuCommandValidator()
        {
            RuleFor(x => x.UpdateMenuRequest)
           .NotNull()
           .WithMessage("Menu data is required.");

            RuleFor(x => x.UpdateMenuRequest.Id)
               .NotEmpty().WithMessage("Menu ID is required.");


            RuleFor(x => x.UpdateMenuRequest.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.UpdateMenuRequest.Type)
                .NotEmpty()
                .WithMessage("Type is required.")
                .Must(type => type == "Top" || type == "Bottom")
                .WithMessage("Type must be either 'Top' or 'Bottom'.");

            RuleFor(x => x.UpdateMenuRequest.Order)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Order must be zero or a positive number.");

            RuleFor(x => x.UpdateMenuRequest.Link)
                .NotNull()
                .WithMessage("Link is required.")
                .NotEmpty()
                .WithMessage("Link is required.")
                .MaximumLength(500)
                .WithMessage("Link cannot exceed 500 characters.");
        }
    }
}

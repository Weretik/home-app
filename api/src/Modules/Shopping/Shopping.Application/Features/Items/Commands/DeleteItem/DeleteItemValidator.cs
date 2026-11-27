using FluentValidation;

namespace Shopping.Application.Features.Items.Commands.DeleteItem;

internal sealed class DeleteItemValidator : AbstractValidator<DeleteItemCommand>
{
    public DeleteItemValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію товару у заголовку If-Match.");
    }
}

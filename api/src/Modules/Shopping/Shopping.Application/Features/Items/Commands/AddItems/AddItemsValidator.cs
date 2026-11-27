using FluentValidation;
using Shopping.Application.Idempotency;
using Shopping.Domain.Items;
using Shopping.Domain.Parsing;

namespace Shopping.Application.Features.Items.Commands.AddItems;

internal sealed class AddItemsValidator : AbstractValidator<AddItemsCommand>
{
    public const int MaxTextLength = 1000;

    public AddItemsValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Text).NotEmpty().MaximumLength(MaxTextLength);
        RuleFor(c => c.Text)
            .Must(text => ItemTextParser.Parse(text).Count > 0).WithMessage("У тексті немає жодного товару.")
            .Must(text => ItemTextParser.Parse(text).Count <= ShoppingItem.MaxItemsPerAdd)
            .WithMessage($"За раз можна додати не більше {ShoppingItem.MaxItemsPerAdd} товарів.")
            .When(c => !string.IsNullOrWhiteSpace(c.Text) && c.Text.Length <= MaxTextLength);
        RuleFor(c => c.AddedBy).IsInEnum();
        RuleFor(c => c.DepartmentId).NotEqual(Guid.Empty);
    }
}

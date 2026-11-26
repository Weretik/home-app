using FluentValidation;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Commands.UpdateItem;

internal sealed class UpdateItemValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the item you edited in the If-Match header.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(ShoppingItem.MaxNameLength);
        RuleFor(c => c.Quantity).MaximumLength(ShoppingItem.MaxQuantityLength);
        RuleFor(c => c.Note).MaximumLength(ShoppingItem.MaxNoteLength);
        RuleFor(c => c.DepartmentId).NotEmpty();
    }
}

using FluentValidation;
using Gifts.Domain.Categories;

namespace Gifts.Application.Features.Categories.Commands.RenameCategory;

internal sealed class RenameCategoryValidator : AbstractValidator<RenameCategoryCommand>
{
    public RenameCategoryValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Category.MaxNameLength);
    }
}

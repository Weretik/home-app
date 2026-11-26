using FluentValidation;

namespace Gifts.Application.Features.Categories.Commands.DeleteCategory;

internal sealed class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator() => RuleFor(c => c.Id).NotEmpty();
}

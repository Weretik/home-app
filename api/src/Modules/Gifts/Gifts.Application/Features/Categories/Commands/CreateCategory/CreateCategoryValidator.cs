using FluentValidation;
using Gifts.Application.Idempotency;
using Gifts.Domain.Categories;

namespace Gifts.Application.Features.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Category.MaxNameLength);
    }
}

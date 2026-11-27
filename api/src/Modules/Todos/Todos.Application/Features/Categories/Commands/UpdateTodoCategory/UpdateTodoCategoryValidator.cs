using FluentValidation;
using Todos.Domain.Categories;

namespace Todos.Application.Features.Categories.Commands.UpdateTodoCategory;

internal sealed class UpdateTodoCategoryValidator : AbstractValidator<UpdateTodoCategoryCommand>
{
    public UpdateTodoCategoryValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненої категорії у заголовку If-Match.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(TodoCategory.MaxNameLength);
        RuleFor(c => c.Emoji).MaximumLength(TodoCategory.MaxEmojiLength);
    }
}

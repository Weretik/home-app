using FluentValidation;
using Todos.Domain.Categories;

namespace Todos.Application.Features.Categories.Commands.CreateTodoCategory;

internal sealed class CreateTodoCategoryValidator : AbstractValidator<CreateTodoCategoryCommand>
{
    public CreateTodoCategoryValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(TodoCategory.MaxNameLength);
        RuleFor(c => c.Emoji).MaximumLength(TodoCategory.MaxEmojiLength);
    }
}

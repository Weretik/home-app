using FluentValidation;
using Todos.Domain.Categories;

namespace Todos.Application.Features.Categories.Commands.UpdateTodoCategory;

internal sealed class UpdateTodoCategoryValidator : AbstractValidator<UpdateTodoCategoryCommand>
{
    public UpdateTodoCategoryValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the category you edited in the If-Match header.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(TodoCategory.MaxNameLength);
        RuleFor(c => c.Emoji).MaximumLength(TodoCategory.MaxEmojiLength);
    }
}

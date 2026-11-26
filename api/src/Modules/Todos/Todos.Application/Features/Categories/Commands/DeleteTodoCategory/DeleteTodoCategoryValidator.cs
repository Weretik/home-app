using FluentValidation;

namespace Todos.Application.Features.Categories.Commands.DeleteTodoCategory;

internal sealed class DeleteTodoCategoryValidator : AbstractValidator<DeleteTodoCategoryCommand>
{
    public DeleteTodoCategoryValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the category you delete in the If-Match header.");
    }
}

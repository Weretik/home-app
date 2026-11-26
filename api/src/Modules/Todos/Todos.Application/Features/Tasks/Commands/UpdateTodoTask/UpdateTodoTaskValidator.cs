using FluentValidation;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.UpdateTodoTask;

internal sealed class UpdateTodoTaskValidator : AbstractValidator<UpdateTodoTaskCommand>
{
    public UpdateTodoTaskValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the task you edited in the If-Match header.");
        RuleFor(c => c.Title).NotEmpty().MaximumLength(TodoTask.MaxTitleLength);
        RuleFor(c => c.Notes).MaximumLength(TodoTask.MaxNotesLength);
        RuleFor(c => c.Assignee).IsInEnum();
        RuleFor(c => c.Author).IsInEnum();
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.Priority).IsInEnum();
        RuleFor(c => c.CategoryId).NotEqual(Guid.Empty);
    }
}

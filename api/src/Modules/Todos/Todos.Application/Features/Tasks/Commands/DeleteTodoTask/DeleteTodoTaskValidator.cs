using FluentValidation;

namespace Todos.Application.Features.Tasks.Commands.DeleteTodoTask;

internal sealed class DeleteTodoTaskValidator : AbstractValidator<DeleteTodoTaskCommand>
{
    public DeleteTodoTaskValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the task you delete in the If-Match header.");
    }
}

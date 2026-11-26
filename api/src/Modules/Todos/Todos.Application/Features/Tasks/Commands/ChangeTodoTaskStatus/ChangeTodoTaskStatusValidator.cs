using FluentValidation;

namespace Todos.Application.Features.Tasks.Commands.ChangeTodoTaskStatus;

internal sealed class ChangeTodoTaskStatusValidator : AbstractValidator<ChangeTodoTaskStatusCommand>
{
    public ChangeTodoTaskStatusValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the task you changed in the If-Match header.");
        RuleFor(c => c.Status).IsInEnum();
    }
}

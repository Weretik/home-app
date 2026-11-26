using FluentValidation;

namespace Todos.Application.Features.Steps.Commands.DeleteTodoTaskStep;

internal sealed class DeleteTodoTaskStepValidator : AbstractValidator<DeleteTodoTaskStepCommand>
{
    public DeleteTodoTaskStepValidator()
    {
        RuleFor(c => c.TaskId).NotEmpty();
        RuleFor(c => c.StepId).NotEmpty();
    }
}

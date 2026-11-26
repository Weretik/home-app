using FluentValidation;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Steps.Commands.UpdateTodoTaskStep;

internal sealed class UpdateTodoTaskStepValidator : AbstractValidator<UpdateTodoTaskStepCommand>
{
    public UpdateTodoTaskStepValidator()
    {
        RuleFor(c => c.TaskId).NotEmpty();
        RuleFor(c => c.StepId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(TodoTask.MaxTitleLength);
    }
}

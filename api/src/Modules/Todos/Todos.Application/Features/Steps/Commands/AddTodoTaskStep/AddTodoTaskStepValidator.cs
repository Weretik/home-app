using FluentValidation;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Steps.Commands.AddTodoTaskStep;

internal sealed class AddTodoTaskStepValidator : AbstractValidator<AddTodoTaskStepCommand>
{
    public AddTodoTaskStepValidator()
    {
        RuleFor(c => c.TaskId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(TodoTask.MaxTitleLength);
    }
}

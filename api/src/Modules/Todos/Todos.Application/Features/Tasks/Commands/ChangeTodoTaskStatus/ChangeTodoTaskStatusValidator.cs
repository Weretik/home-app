using FluentValidation;

namespace Todos.Application.Features.Tasks.Commands.ChangeTodoTaskStatus;

internal sealed class ChangeTodoTaskStatusValidator : AbstractValidator<ChangeTodoTaskStatusCommand>
{
    public ChangeTodoTaskStatusValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненого завдання у заголовку If-Match.");
        RuleFor(c => c.Status).IsInEnum();
    }
}

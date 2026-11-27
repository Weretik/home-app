using FluentValidation;

namespace Todos.Application.Features.Tasks.Commands.DeleteTodoTask;

internal sealed class DeleteTodoTaskValidator : AbstractValidator<DeleteTodoTaskCommand>
{
    public DeleteTodoTaskValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію завдання, яке видаляєте, у заголовку If-Match.");
    }
}

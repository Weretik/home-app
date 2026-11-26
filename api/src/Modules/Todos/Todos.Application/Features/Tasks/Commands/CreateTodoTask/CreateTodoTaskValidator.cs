using FluentValidation;
using Todos.Application.Idempotency;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.CreateTodoTask;

internal sealed class CreateTodoTaskValidator : AbstractValidator<CreateTodoTaskCommand>
{
    public CreateTodoTaskValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Title).NotEmpty().MaximumLength(TodoTask.MaxTitleLength);
        RuleFor(c => c.Notes).MaximumLength(TodoTask.MaxNotesLength);
        RuleFor(c => c.Assignee).IsInEnum();
        RuleFor(c => c.Author).IsInEnum();
        RuleFor(c => c.Status).IsInEnum();
        RuleFor(c => c.Priority).IsInEnum();
        RuleFor(c => c.CategoryId).NotEqual(Guid.Empty);
        RuleFor(c => c.Steps).Must(s => s.Count <= TodoTask.MaxSteps)
            .WithMessage($"A task has at most {TodoTask.MaxSteps} steps.");
        RuleForEach(c => c.Steps).NotEmpty().MaximumLength(TodoTask.MaxTitleLength);
    }
}

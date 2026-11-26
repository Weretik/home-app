using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Application.Idempotency;

namespace Todos.Application.Features.Steps.Commands.AddTodoTaskStep;

public sealed class AddTodoTaskStepHandler(ITodosDbContext db, TimeProvider clock)
    : ICommandHandler<AddTodoTaskStepCommand, Result<TodoCreated>>
{
    public async ValueTask<Result<TodoCreated>> Handle(AddTodoTaskStepCommand c, CancellationToken cancellationToken)
    {
        var stepId = Guid.Empty;
        var saved = await StepChanges.ApplyAsync(
            db,
            c.TaskId,
            task =>
            {
                var added = task.AddStep(c.Title, clock.GetUtcNow());
                if (added.IsFailure)
                    return added.Error!;

                stepId = added.Value.Id;
                return Result.Success();
            },
            cancellationToken);

        return saved.IsFailure ? saved.Error! : new TodoCreated(stepId);
    }
}

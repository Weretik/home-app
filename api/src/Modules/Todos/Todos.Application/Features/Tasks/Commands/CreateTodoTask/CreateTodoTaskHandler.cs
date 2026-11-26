using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Idempotency;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Commands.CreateTodoTask;

public sealed class CreateTodoTaskHandler(ITodosDbContext db, TimeProvider clock)
    : ICommandHandler<CreateTodoTaskCommand, Result<TodoCreated>>
{
    private const string Kind = "task";

    public async ValueTask<Result<TodoCreated>> Handle(CreateTodoTaskCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[]
        {
            c.Title, c.Notes, c.Assignee, c.Author, c.Status, c.Priority, c.DueDate, c.CategoryId, c.Steps,
        });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        if (c.CategoryId is { } categoryId && !await db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken))
            return CategoryErrors.NotFound(categoryId);

        var now = clock.GetUtcNow();
        var created = TodoTask.Create(
            c.Title, c.Notes, c.Assignee, c.Author, c.Status ?? TodoStatus.Todo, c.Priority, c.DueDate, c.CategoryId, c.Steps, now);
        if (created.IsFailure)
            return created.Error!;

        db.Tasks.Add(created.Value);
        db.CreateRequests.Add(new CreateRequest(c.IdempotencyKey, Kind, fingerprint, created.Value.Id, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A racing copy of the same request saved first: answer like a repeat.
            return await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken)
                ?? IdempotentCreates.KeyReused;
        }

        return new TodoCreated(created.Value.Id);
    }
}

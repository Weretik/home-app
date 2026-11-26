using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Idempotency;
using Todos.Domain.Categories;

namespace Todos.Application.Features.Categories.Commands.CreateTodoCategory;

public sealed class CreateTodoCategoryHandler(ITodosDbContext db, TimeProvider clock)
    : ICommandHandler<CreateTodoCategoryCommand, Result<TodoCreated>>
{
    public async ValueTask<Result<TodoCreated>> Handle(CreateTodoCategoryCommand c, CancellationToken cancellationToken)
    {
        var name = c.Name.Trim().ToLowerInvariant();
        if (await db.Categories.AnyAsync(x => x.Name.ToLower() == name, cancellationToken))
            return CategoryErrors.NameTaken;

        var last = await db.Categories.MaxAsync(x => (int?)x.SortOrder, cancellationToken) ?? 0;
        var created = TodoCategory.Create(Guid.CreateVersion7(clock.GetUtcNow()), c.Name, c.Emoji, last + 10);
        if (created.IsFailure)
            return created.Error!;

        db.Categories.Add(created.Value);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // Another request took the name between the check and the save.
            return CategoryErrors.NameTaken;
        }

        return new TodoCreated(created.Value.Id);
    }
}

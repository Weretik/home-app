using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Categories.Commands.DeleteTodoCategory;

public sealed class DeleteTodoCategoryHandler(ITodosDbContext db) : ICommandHandler<DeleteTodoCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteTodoCategoryCommand c, CancellationToken cancellationToken)
    {
        // One conditional DELETE; the database clears category_id of its tasks (ON DELETE SET NULL).
        var deleted = await db.Categories.Where(x => x.Id == c.Id && x.Version == c.Version).ExecuteDeleteAsync(cancellationToken);
        if (deleted > 0)
            return Result.Success();

        return await db.Categories.AnyAsync(x => x.Id == c.Id, cancellationToken)
            ? TodoErrors.ChangedMeanwhile
            : CategoryErrors.NotFound(c.Id);
    }
}

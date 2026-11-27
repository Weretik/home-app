using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Features.Tasks.Reading;
using Todos.Domain.Tasks;
using Todos.Domain.Tasks.Identifiers;

namespace Todos.Application.Features.Tasks.Queries.GetTodoTask;

public sealed class GetTodoTaskHandler(ITodosDbContext db) : IQueryHandler<GetTodoTaskQuery, Result<TodoTaskItem>>
{
    public async ValueTask<Result<TodoTaskItem>> Handle(GetTodoTaskQuery query, CancellationToken cancellationToken)
    {
        var item = await db.Tasks.AsNoTracking()
            .Where(t => t.Id == new TodoTaskId(query.Id))
            .WithCategory(db.Categories.AsNoTracking())
            .Select(TodoTaskProjection.ToItem)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? TodoErrors.TaskNotFound(new TodoTaskId(query.Id)) : item;
    }
}

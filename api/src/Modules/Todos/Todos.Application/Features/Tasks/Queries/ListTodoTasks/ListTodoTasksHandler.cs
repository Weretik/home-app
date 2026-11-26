using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Application.Features.Tasks.Reading;

namespace Todos.Application.Features.Tasks.Queries.ListTodoTasks;

public sealed class ListTodoTasksHandler(ITodosDbContext db) : IQueryHandler<ListTodoTasksQuery, Result<TodoTaskList>>
{
    public async ValueTask<Result<TodoTaskList>> Handle(ListTodoTasksQuery query, CancellationToken cancellationToken)
    {
        var archive = query.View == TaskViews.ArchiveView;
        var tasks = archive ? db.Tasks.AsNoTracking().WhereArchived() : db.Tasks.AsNoTracking().WhereActive();

        if (query.Assignees.Count > 0)
        {
            var assignees = query.Assignees.Distinct().ToList();
            tasks = tasks.Where(t => assignees.Contains(t.Assignee));
        }

        if (query.CategoryId is { } categoryId)
            tasks = tasks.Where(t => t.CategoryId == categoryId);
        else if (query.WithoutCategory)
            tasks = tasks.Where(t => t.CategoryId == null);

        foreach (var pattern in SearchPattern.ContainsEachWord(query.Search))
            tasks = tasks.Where(t => EF.Functions.Like(t.Title.ToLower(), pattern, SearchPattern.EscapeCharacter));

        var rows = tasks.WithCategory(db.Categories.AsNoTracking());
        var page = await (archive ? rows.Archive() : rows.Active())
            .Select(TodoTaskProjection.ToItem)
            .ToPagedListAsync(query.Page, query.PageSize, cancellationToken);

        return new TodoTaskList(page.Items, page.Page, page.PageSize, page.TotalCount);
    }
}

using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Tasks.Queries.ListTodoTasks;
using Todos.Application.Features.Tasks.Reading;
using Todos.Domain.Parties;

namespace Todos.Api.Endpoints.Tasks.Reading;

internal static class ListTodoTasksEndpoint
{
    public static RouteGroupBuilder MapListTodoTasks(this RouteGroupBuilder group)
    {
        group.MapGet("/tasks", HandleAsync)
            .WithName("listTodoTasks")
            .WithSummary("Lists the active tasks (view=active, by priority, due date, creation) or the archive (view=archive, newest completion first). assignee may repeat.")
            .Produces<TodoTaskList>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] string? view,
        [FromQuery] Assignee[]? assignee,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? withoutCategory,
        [FromQuery] string? search,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 50)
    {
        var result = await sender.Send(
            new ListTodoTasksQuery(
                (view ?? TaskViews.ActiveView).Trim().ToLowerInvariant(),
                assignee ?? [],
                categoryId,
                withoutCategory ?? false,
                search,
                page,
                pageSize),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Summary.Queries.GetTodoSummary;

namespace Todos.Api.Endpoints.Tasks.Reading;

internal static class GetTodoSummaryEndpoint
{
    public static RouteGroupBuilder MapGetTodoSummary(this RouteGroupBuilder group)
    {
        group.MapGet("/summary", HandleAsync)
            .WithName("getTodoSummary")
            .WithSummary("The home page block: per assignee the active, overdue and due-today counts, tasks done in 7 days, and up to 8 tasks to look at first. Today is the date in Kyiv.")
            .Produces<TodoSummary>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTodoSummaryQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

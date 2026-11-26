using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Tasks.Queries.GetTodoTask;
using Todos.Application.Features.Tasks.Reading;

namespace Todos.Api.Endpoints.Tasks.Reading;

internal static class GetTodoTaskEndpoint
{
    public static RouteGroupBuilder MapGetTodoTask(this RouteGroupBuilder group)
    {
        group.MapGet("/tasks/{id:guid}", HandleAsync)
            .WithName("getTodoTask")
            .WithSummary("Returns one task with its steps, active or archived.")
            .Produces<TodoTaskItem>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
        return group;
    }

    private static async Task<IResult> HandleAsync(Guid id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTodoTaskQuery(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

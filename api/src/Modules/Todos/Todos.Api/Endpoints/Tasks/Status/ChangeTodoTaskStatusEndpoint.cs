using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Tasks.Commands.ChangeTodoTaskStatus;

namespace Todos.Api.Endpoints.Tasks.Status;

internal static class ChangeTodoTaskStatusEndpoint
{
    public static RouteGroupBuilder MapChangeTodoTaskStatus(this RouteGroupBuilder group)
    {
        group.MapPut("/tasks/{id:guid}/status", HandleAsync)
            .WithName("changeTodoTaskStatus")
            .WithSummary("Moves a task to another status, also out of the archive. Send its version in If-Match; 409 when someone changed it meanwhile.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        [FromHeader(Name = TodoVersionHeader.Name)] string? ifMatch,
        TodoTaskStatusRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeTodoTaskStatusCommand(id, TodoVersionHeader.Parse(ifMatch), request.Status), cancellationToken);
        return result.ToHttpResult(http);
    }
}

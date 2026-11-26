using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Tasks.Commands.DeleteTodoTask;

namespace Todos.Api.Endpoints.Tasks.Editing;

internal static class DeleteTodoTaskEndpoint
{
    public static RouteGroupBuilder MapDeleteTodoTask(this RouteGroupBuilder group)
    {
        group.MapDelete("/tasks/{id:guid}", HandleAsync)
            .WithName("deleteTodoTask")
            .WithSummary("Deletes a task and its steps for good. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTodoTaskCommand(id, TodoVersionHeader.Parse(ifMatch)), cancellationToken);
        return result.ToHttpResult(http);
    }
}

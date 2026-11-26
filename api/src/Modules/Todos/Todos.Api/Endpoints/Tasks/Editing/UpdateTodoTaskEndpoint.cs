using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Tasks.Commands.UpdateTodoTask;

namespace Todos.Api.Endpoints.Tasks.Editing;

internal static class UpdateTodoTaskEndpoint
{
    public static RouteGroupBuilder MapUpdateTodoTask(this RouteGroupBuilder group)
    {
        group.MapPut("/tasks/{id:guid}", HandleAsync)
            .WithName("updateTodoTask")
            .WithSummary("Changes a task, status included. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        TodoTaskUpdateRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTodoTaskCommand(
                id,
                TodoVersionHeader.Parse(ifMatch),
                request.Title ?? string.Empty,
                request.Notes,
                request.Assignee,
                request.Author,
                request.Status,
                request.Priority,
                request.DueDate,
                request.CategoryId),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

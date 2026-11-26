using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Steps.Commands.UpdateTodoTaskStep;

namespace Todos.Api.Endpoints.Steps;

internal static class UpdateTodoTaskStepEndpoint
{
    public static RouteGroupBuilder MapUpdateTodoTaskStep(this RouteGroupBuilder group)
    {
        group.MapPut("/tasks/{id:guid}/steps/{stepId:guid}", HandleAsync)
            .WithName("updateTodoTaskStep")
            .WithSummary("Renames a step and ticks or unticks it. Needs no If-Match.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id, Guid stepId, TodoStepUpdateRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTodoTaskStepCommand(id, stepId, request.Title ?? string.Empty, request.IsDone), cancellationToken);
        return result.ToHttpResult(http);
    }
}

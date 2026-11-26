using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Steps.Commands.DeleteTodoTaskStep;

namespace Todos.Api.Endpoints.Steps;

internal static class DeleteTodoTaskStepEndpoint
{
    public static RouteGroupBuilder MapDeleteTodoTaskStep(this RouteGroupBuilder group)
    {
        group.MapDelete("/tasks/{id:guid}/steps/{stepId:guid}", HandleAsync)
            .WithName("deleteTodoTaskStep")
            .WithSummary("Removes a step from a task's checklist. Needs no If-Match.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id, Guid stepId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteTodoTaskStepCommand(id, stepId), cancellationToken);
        return result.ToHttpResult(http);
    }
}

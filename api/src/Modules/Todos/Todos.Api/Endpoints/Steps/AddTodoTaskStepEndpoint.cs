using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Steps.Commands.AddTodoTaskStep;
using Todos.Application.Idempotency;

namespace Todos.Api.Endpoints.Steps;

internal static class AddTodoTaskStepEndpoint
{
    public static RouteGroupBuilder MapAddTodoTaskStep(this RouteGroupBuilder group)
    {
        group.MapPost("/tasks/{id:guid}/steps", HandleAsync)
            .WithName("addTodoTaskStep")
            .WithSummary("Adds a step at the end of a task's checklist (at most 50). Needs no If-Match.")
            .Produces<TodoCreated>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id, TodoStepRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddTodoTaskStepCommand(id, request.Title ?? string.Empty), cancellationToken);
        return result.ToCreatedHttpResult(http, _ => $"/api/todos/tasks/{id}");
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Tasks.Commands.CreateTodoTask;
using Todos.Application.Idempotency;

namespace Todos.Api.Endpoints.Tasks.Editing;

internal static class CreateTodoTaskEndpoint
{
    public static RouteGroupBuilder MapCreateTodoTask(this RouteGroupBuilder group)
    {
        group.MapPost("/tasks", HandleAsync)
            .WithName("createTodoTask")
            .WithSummary("Adds a task with its first steps. Send a new Idempotency-Key per task; a repeat returns the first id.")
            .Produces<TodoCreated>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        TodoTaskRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateTodoTaskCommand(
                idempotencyKey ?? string.Empty,
                request.Title ?? string.Empty,
                request.Notes,
                request.Assignee,
                request.Author,
                request.Status,
                request.Priority,
                request.DueDate,
                request.CategoryId,
                request.Steps ?? []),
            cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/todos/tasks/{created.Id}");
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Categories.Commands.CreateTodoCategory;
using Todos.Application.Idempotency;

namespace Todos.Api.Endpoints.Categories;

internal static class CreateTodoCategoryEndpoint
{
    public static RouteGroupBuilder MapCreateTodoCategory(this RouteGroupBuilder group)
    {
        group.MapPost("/categories", HandleAsync)
            .WithName("createTodoCategory")
            .WithSummary("Adds a category at the end of the list. 409 when the name (any case) is taken.")
            .Produces<TodoCreated>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        TodoCategoryRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateTodoCategoryCommand(request.Name ?? string.Empty, request.Emoji), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/todos/categories/{created.Id}");
    }
}

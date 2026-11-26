using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Categories.Commands.UpdateTodoCategory;

namespace Todos.Api.Endpoints.Categories;

internal static class UpdateTodoCategoryEndpoint
{
    public static RouteGroupBuilder MapUpdateTodoCategory(this RouteGroupBuilder group)
    {
        group.MapPut("/categories/{id:guid}", HandleAsync)
            .WithName("updateTodoCategory")
            .WithSummary("Renames a category and changes its emoji. Send its version in If-Match; 409 when changed meanwhile or the name is taken.")
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
        TodoCategoryRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateTodoCategoryCommand(id, TodoVersionHeader.Parse(ifMatch), request.Name ?? string.Empty, request.Emoji),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

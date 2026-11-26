using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Categories.Commands.DeleteTodoCategory;

namespace Todos.Api.Endpoints.Categories;

internal static class DeleteTodoCategoryEndpoint
{
    public static RouteGroupBuilder MapDeleteTodoCategory(this RouteGroupBuilder group)
    {
        group.MapDelete("/categories/{id:guid}", HandleAsync)
            .WithName("deleteTodoCategory")
            .WithSummary("Deletes a category; its tasks stay, without a category. Send its version in If-Match; 409 when changed meanwhile.")
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
        var result = await sender.Send(new DeleteTodoCategoryCommand(id, TodoVersionHeader.Parse(ifMatch)), cancellationToken);
        return result.ToHttpResult(http);
    }
}

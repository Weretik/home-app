using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Commands.DeleteItem;

namespace Shopping.Api.Endpoints.Items.Editing;

internal static class DeleteShoppingItemEndpoint
{
    public static RouteGroupBuilder MapDeleteShoppingItem(this RouteGroupBuilder group)
    {
        group.MapDelete("/items/{id:guid}", HandleAsync)
            .WithName("deleteShoppingItem")
            .WithSummary("Deletes a list or history row for good. Send its version in If-Match.")
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
        [FromHeader(Name = VersionHeader.Name)] string? ifMatch,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteItemCommand(id, VersionHeader.Parse(ifMatch)), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Categories.Commands.DeleteCategory;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Categories;

internal static class DeleteGiftCategoryEndpoint
{
    public static RouteGroupBuilder MapDeleteGiftCategory(this RouteGroupBuilder group)
    {
        group.MapDelete("/categories/{id:guid}", HandleAsync)
            .WithName("deleteGiftCategory")
            .WithSummary("Deletes a gift category; 409 while any gift (also an archived one) uses it.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(Guid id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}

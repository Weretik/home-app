using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Categories.Commands.RenameCategory;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Categories;

internal static class RenameGiftCategoryEndpoint
{
    public static RouteGroupBuilder MapRenameGiftCategory(this RouteGroupBuilder group)
    {
        group.MapPut("/categories/{id:guid}", HandleAsync)
            .WithName("renameGiftCategory")
            .WithSummary("Renames a gift category; its gifts follow. 409 when the name is taken.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id, GiftCategoryRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RenameCategoryCommand(id, request.Name), cancellationToken);
        return result.ToHttpResult(http);
    }
}

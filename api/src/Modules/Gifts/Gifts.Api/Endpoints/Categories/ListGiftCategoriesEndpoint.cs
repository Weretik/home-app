using BuildingBlocks.Api.Problems;
using Gifts.Application.Features.Categories.Queries.ListCategories;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Categories;

internal static class ListGiftCategoriesEndpoint
{
    public static RouteGroupBuilder MapListGiftCategories(this RouteGroupBuilder group)
    {
        group.MapGet("/categories", HandleAsync)
            .WithName("listGiftCategories")
            .WithSummary("Lists the gift categories in display order.")
            .Produces<IReadOnlyList<GiftCategoryItem>>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListCategoriesQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

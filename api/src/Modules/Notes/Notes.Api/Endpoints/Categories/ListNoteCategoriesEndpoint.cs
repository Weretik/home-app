using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Categories.Queries.ListNoteCategories;

namespace Notes.Api.Endpoints.Categories;

internal static class ListNoteCategoriesEndpoint
{
    public static RouteGroupBuilder MapListNoteCategories(this RouteGroupBuilder group)
    {
        group.MapGet("/categories", HandleAsync)
            .WithName("listNoteCategories")
            .WithSummary("Lists the note categories in display order, with how many notes each holds.")
            .Produces<IReadOnlyList<NoteCategoryItem>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListNoteCategoriesQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

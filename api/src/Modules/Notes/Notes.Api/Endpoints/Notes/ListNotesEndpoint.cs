using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Notes.Queries.ListNotes;
using Notes.Domain.Notes;

namespace Notes.Api.Endpoints.Notes;

internal static class ListNotesEndpoint
{
    public static RouteGroupBuilder MapListNotes(this RouteGroupBuilder group)
    {
        group.MapGet("/items", HandleAsync)
            .WithName("listNotes")
            .WithSummary("Lists the notes by category order (none last), important first, then Likes, Dislikes, Fact, then title; at most 1000. about narrows to one person, kind may repeat, categoryId or withoutCategory narrow by category, every word of search must be in the title, description or place.")
            .Produces<IReadOnlyList<NoteItem>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] NotePerson? about,
        [FromQuery] NoteKind[]? kind,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? withoutCategory,
        [FromQuery] string? search,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ListNotesQuery(about, kind ?? [], categoryId, withoutCategory ?? false, search),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Notes.Commands.DeleteNote;

namespace Notes.Api.Endpoints.Notes;

internal static class DeleteNoteEndpoint
{
    public static RouteGroupBuilder MapDeleteNote(this RouteGroupBuilder group)
    {
        group.MapDelete("/items/{id:guid}", HandleAsync)
            .WithName("deleteNote")
            .WithSummary("Deletes a note for good. Send its version in If-Match; 409 when someone changed it meanwhile.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        [FromHeader(Name = NoteVersionHeader.Name)] string? ifMatch,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteNoteCommand(id, NoteVersionHeader.Parse(ifMatch)), cancellationToken);
        return result.ToHttpResult(http);
    }
}

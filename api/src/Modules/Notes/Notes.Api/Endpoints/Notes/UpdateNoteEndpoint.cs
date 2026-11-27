using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Notes.Commands.UpdateNote;

namespace Notes.Api.Endpoints.Notes;

internal static class UpdateNoteEndpoint
{
    public static RouteGroupBuilder MapUpdateNote(this RouteGroupBuilder group)
    {
        group.MapPut("/items/{id:guid}", HandleAsync)
            .WithName("updateNote")
            .WithSummary("Changes a note; the signed-in member becomes its last editor. Send its version in If-Match; 409 when someone changed it meanwhile.")
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
        NoteRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateNoteCommand(
                id,
                NoteVersionHeader.Parse(ifMatch),
                request.About,
                request.Kind,
                request.Title ?? string.Empty,
                request.Details,
                request.Place,
                request.CategoryId,
                request.IsImportant),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

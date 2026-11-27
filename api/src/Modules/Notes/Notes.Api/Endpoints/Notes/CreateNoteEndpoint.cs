using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Notes.Commands.CreateNote;
using Notes.Application.Idempotency;

namespace Notes.Api.Endpoints.Notes;

internal static class CreateNoteEndpoint
{
    public static RouteGroupBuilder MapCreateNote(this RouteGroupBuilder group)
    {
        group.MapPost("/items", HandleAsync)
            .WithName("createNote")
            .WithSummary("Adds a note about Vitaliy or Maria; the author is the signed-in member. Send a new Idempotency-Key per note; a repeat returns the first id.")
            .Produces<NoteCreated>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        NoteRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateNoteCommand(
                idempotencyKey ?? string.Empty,
                request.About,
                request.Kind,
                request.Title ?? string.Empty,
                request.Details,
                request.Place,
                request.CategoryId,
                request.IsImportant),
            cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/notes/items/{created.Id}");
    }
}

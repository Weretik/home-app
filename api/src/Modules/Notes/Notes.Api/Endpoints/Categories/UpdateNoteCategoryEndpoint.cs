using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Categories.Commands.UpdateNoteCategory;

namespace Notes.Api.Endpoints.Categories;

internal static class UpdateNoteCategoryEndpoint
{
    public static RouteGroupBuilder MapUpdateNoteCategory(this RouteGroupBuilder group)
    {
        group.MapPut("/categories/{id:guid}", HandleAsync)
            .WithName("updateNoteCategory")
            .WithSummary("Renames a category and changes its emoji. Send its version in If-Match; 409 when changed meanwhile or the name is taken.")
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
        NoteCategoryRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateNoteCategoryCommand(id, NoteVersionHeader.Parse(ifMatch), request.Name ?? string.Empty, request.Emoji),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

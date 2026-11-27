using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Categories.Commands.DeleteNoteCategory;

namespace Notes.Api.Endpoints.Categories;

internal static class DeleteNoteCategoryEndpoint
{
    public static RouteGroupBuilder MapDeleteNoteCategory(this RouteGroupBuilder group)
    {
        group.MapDelete("/categories/{id:guid}", HandleAsync)
            .WithName("deleteNoteCategory")
            .WithSummary("Deletes a category; its notes stay, without a category. Send its version in If-Match; 409 when changed meanwhile.")
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
        var result = await sender.Send(new DeleteNoteCategoryCommand(id, NoteVersionHeader.Parse(ifMatch)), cancellationToken);
        return result.ToHttpResult(http);
    }
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notes.Application.Features.Categories.Commands.CreateNoteCategory;
using Notes.Application.Idempotency;

namespace Notes.Api.Endpoints.Categories;

internal static class CreateNoteCategoryEndpoint
{
    public static RouteGroupBuilder MapCreateNoteCategory(this RouteGroupBuilder group)
    {
        group.MapPost("/categories", HandleAsync)
            .WithName("createNoteCategory")
            .WithSummary("Adds a category at the end of the list. 409 when the name (any case) is taken.")
            .Produces<NoteCreated>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        NoteCategoryRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateNoteCategoryCommand(request.Name ?? string.Empty, request.Emoji), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/notes/categories/{created.Id}");
    }
}

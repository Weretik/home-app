using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Categories.Commands.CreateCategory;
using Gifts.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Categories;

internal static class CreateGiftCategoryEndpoint
{
    public static RouteGroupBuilder MapCreateGiftCategory(this RouteGroupBuilder group)
    {
        group.MapPost("/categories", HandleAsync)
            .WithName("createGiftCategory")
            .WithSummary("Adds a gift category at the end of the list. Send a new Idempotency-Key; 409 when the name is taken.")
            .Produces<CreatedGiftsEntry>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        GiftCategoryRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCategoryCommand(idempotencyKey ?? string.Empty, request.Name), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/gifts/categories/{created.Id}");
    }
}

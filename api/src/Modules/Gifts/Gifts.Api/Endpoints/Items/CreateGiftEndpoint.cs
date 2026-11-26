using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Items.Commands.CreateGift;
using Gifts.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Items;

internal static class CreateGiftEndpoint
{
    public static RouteGroupBuilder MapCreateGift(this RouteGroupBuilder group)
    {
        group.MapPost("/items", HandleAsync)
            .WithName("createGift")
            .WithSummary("Adds a wanted gift with its variants. Send a new Idempotency-Key per gift; a repeat returns the first id.")
            .Produces<CreatedGiftsEntry>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        GiftRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateGiftCommand(
                idempotencyKey ?? string.Empty,
                request.Recipient,
                request.Name,
                request.CategoryId,
                request.Priority,
                request.Price,
                request.Link,
                request.Occasion,
                request.OccasionDate,
                request.Notes,
                request.VariantDrafts()),
            cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/gifts/items/{created.Id}");
    }
}

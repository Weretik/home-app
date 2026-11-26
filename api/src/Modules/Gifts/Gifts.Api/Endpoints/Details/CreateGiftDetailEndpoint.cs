using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Gifts.Application.Features.Details.Commands.CreateDetail;
using Gifts.Application.Idempotency;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Gifts.Api.Endpoints.Details;

internal static class CreateGiftDetailEndpoint
{
    public static RouteGroupBuilder MapCreateGiftDetail(this RouteGroupBuilder group)
    {
        group.MapPost("/details", HandleAsync)
            .WithName("createGiftDetail")
            .WithSummary("Adds a detail at the end of the recipient's list. Send a new Idempotency-Key; a repeat returns the first id.")
            .Produces<CreatedGiftsEntry>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        GiftDetailRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CreateDetailCommand(idempotencyKey ?? string.Empty, request.Recipient, request.Label, request.Value), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/gifts/details/{created.Id}");
    }
}

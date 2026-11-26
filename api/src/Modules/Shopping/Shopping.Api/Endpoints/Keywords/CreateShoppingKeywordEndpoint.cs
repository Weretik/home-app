using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Keywords.Commands.CreateKeyword;
using Shopping.Application.Idempotency;

namespace Shopping.Api.Endpoints.Keywords;

internal static class CreateShoppingKeywordEndpoint
{
    public static RouteGroupBuilder MapCreateShoppingKeyword(this RouteGroupBuilder group)
    {
        group.MapPost("/keywords", HandleAsync)
            .WithName("createShoppingKeyword")
            .WithSummary("Adds a keyword (stored normalised). Send a new Idempotency-Key; a repeat returns the first id, a taken keyword 409.")
            .Produces<ShoppingCreatedId>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        Request request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateKeywordCommand(idempotencyKey ?? string.Empty, request.Keyword, request.DepartmentId), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/shopping/keywords/{created.Id}");
    }

    /// <summary>A word start (or several) and the department it points to.</summary>
    internal sealed record Request(string Keyword, Guid DepartmentId);
}

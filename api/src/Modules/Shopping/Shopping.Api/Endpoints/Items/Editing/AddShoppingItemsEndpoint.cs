using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Commands.AddItems;
using Shopping.Domain.Items;

namespace Shopping.Api.Endpoints.Items.Editing;

internal static class AddShoppingItemsEndpoint
{
    public static RouteGroupBuilder MapAddShoppingItems(this RouteGroupBuilder group)
    {
        group.MapPost("/items", HandleAsync)
            .WithName("addShoppingItems")
            .WithSummary("Adds items typed as text («молоко, хлеб, яйца 10»). Send a new Idempotency-Key per add; a repeat returns the first result.")
            .Produces<AddItemsResult>(StatusCodes.Status201Created)
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
        var result = await sender.Send(
            new AddItemsCommand(idempotencyKey ?? string.Empty, request.Text, request.AddedBy, request.DepartmentId, request.Urgent ?? false),
            cancellationToken);
        return result.ToCreatedHttpResult(http, _ => "/api/shopping/items");
    }

    /// <summary>The text to read, who adds it, and optionally one department and «Срочно» for all its items.</summary>
    internal sealed record Request(string Text, Person AddedBy, Guid? DepartmentId, bool? Urgent);
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Commands.SetItemStatus;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;

namespace Shopping.Api.Endpoints.Items.Editing;

internal static class SetShoppingItemStatusEndpoint
{
    public static RouteGroupBuilder MapSetShoppingItemStatus(this RouteGroupBuilder group)
    {
        group.MapPut("/items/{id:guid}/status", HandleAsync)
            .WithName("setShoppingItemStatus")
            .WithSummary("Ticks an item bought or needed. Sets a target value, so a repeat changes nothing; 404 for an item that is history.")
            .Produces<ShoppingItemView>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id, Request request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetItemStatusCommand(id, request.Status, request.By), cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>The new status and who set it.</summary>
    internal sealed record Request(ShoppingItemStatus Status, Person By);
}

using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Items.Commands.UpdateItem;
using Shopping.Application.Features.Items.Views;

namespace Shopping.Api.Endpoints.Items.Editing;

internal static class UpdateShoppingItemEndpoint
{
    public static RouteGroupBuilder MapUpdateShoppingItem(this RouteGroupBuilder group)
    {
        group.MapPut("/items/{id:guid}", HandleAsync)
            .WithName("updateShoppingItem")
            .WithSummary("Changes an item in the list. Send its version in If-Match; moving it to another department teaches the dictionary its name.")
            .Produces<ShoppingItemView>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        [FromHeader(Name = VersionHeader.Name)] string? ifMatch,
        Request request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateItemCommand(id, VersionHeader.Parse(ifMatch), request.Name, request.Quantity, request.Note, request.DepartmentId, request.Urgent),
            cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>The fields of the edit dialog.</summary>
    internal sealed record Request(string Name, string? Quantity, string? Note, Guid DepartmentId, bool Urgent);
}

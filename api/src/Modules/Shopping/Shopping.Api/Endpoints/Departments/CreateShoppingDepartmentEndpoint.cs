using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Departments.Commands.CreateDepartment;
using Shopping.Application.Idempotency;

namespace Shopping.Api.Endpoints.Departments;

internal static class CreateShoppingDepartmentEndpoint
{
    public static RouteGroupBuilder MapCreateShoppingDepartment(this RouteGroupBuilder group)
    {
        group.MapPost("/departments", HandleAsync)
            .WithName("createShoppingDepartment")
            .WithSummary("Adds a department just before the fallback. Send a new Idempotency-Key; a repeat returns the first id.")
            .Produces<ShoppingCreatedId>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
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
        var result = await sender.Send(new CreateDepartmentCommand(idempotencyKey ?? string.Empty, request.Name, request.Icon), cancellationToken);
        return result.ToCreatedHttpResult(http, created => $"/api/shopping/departments/{created.Id}");
    }

    /// <summary>The department's name and its emoji.</summary>
    internal sealed record Request(string Name, string? Icon);
}

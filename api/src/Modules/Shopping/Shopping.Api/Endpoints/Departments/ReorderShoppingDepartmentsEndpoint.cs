using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Departments.Commands.ReorderDepartments;

namespace Shopping.Api.Endpoints.Departments;

internal static class ReorderShoppingDepartmentsEndpoint
{
    public static RouteGroupBuilder MapReorderShoppingDepartments(this RouteGroupBuilder group)
    {
        group.MapPut("/departments/order", HandleAsync)
            .WithName("reorderShoppingDepartments")
            .WithSummary("Sets the store walk: send every department id exactly once, in the new order.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(Request request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReorderDepartmentsCommand(request.DepartmentIds ?? []), cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>All department ids in the new order.</summary>
    internal sealed record Request(IReadOnlyList<Guid>? DepartmentIds);
}

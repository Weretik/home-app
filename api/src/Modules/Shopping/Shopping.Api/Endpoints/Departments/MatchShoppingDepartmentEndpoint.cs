using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Departments.Queries.MatchDepartment;

namespace Shopping.Api.Endpoints.Departments;

internal static class MatchShoppingDepartmentEndpoint
{
    public static RouteGroupBuilder MapMatchShoppingDepartment(this RouteGroupBuilder group)
    {
        group.MapGet("/departments/match", HandleAsync)
            .WithName("matchShoppingDepartment")
            .WithSummary("Says which department an item with this name would go to (the preview while typing).")
            .Produces<DepartmentMatch>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
        return group;
    }

    private static async Task<IResult> HandleAsync(string? name, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MatchDepartmentQuery(name), cancellationToken);
        return result.ToHttpResult(http);
    }
}

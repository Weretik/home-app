using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Departments.Queries.ListDepartments;

namespace Shopping.Api.Endpoints.Departments;

internal static class ListShoppingDepartmentsEndpoint
{
    public static RouteGroupBuilder MapListShoppingDepartments(this RouteGroupBuilder group)
    {
        group.MapGet("/departments", HandleAsync)
            .WithName("listShoppingDepartments")
            .WithSummary("Lists the departments in store order, with their keyword and needed item counts.")
            .Produces<IReadOnlyList<DepartmentItem>>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListDepartmentsQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

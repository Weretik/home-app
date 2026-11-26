using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Departments.Commands.UpdateDepartment;

namespace Shopping.Api.Endpoints.Departments;

internal static class UpdateShoppingDepartmentEndpoint
{
    public static RouteGroupBuilder MapUpdateShoppingDepartment(this RouteGroupBuilder group)
    {
        group.MapPut("/departments/{id:guid}", HandleAsync)
            .WithName("updateShoppingDepartment")
            .WithSummary("Renames a department or changes its icon. Send its version in If-Match.")
            .Produces(StatusCodes.Status204NoContent)
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
        var result = await sender.Send(new UpdateDepartmentCommand(id, VersionHeader.Parse(ifMatch), request.Name, request.Icon), cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>The department's new name and emoji.</summary>
    internal sealed record Request(string Name, string? Icon);
}

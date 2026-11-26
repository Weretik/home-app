using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shopping.Application.Features.Departments.Commands.DeleteDepartment;

namespace Shopping.Api.Endpoints.Departments;

internal static class DeleteShoppingDepartmentEndpoint
{
    public static RouteGroupBuilder MapDeleteShoppingDepartment(this RouteGroupBuilder group)
    {
        group.MapDelete("/departments/{id:guid}", HandleAsync)
            .WithName("deleteShoppingDepartment")
            .WithSummary("Deletes a department; its items and keywords move to the fallback. Send its version in If-Match; the fallback answers 409.")
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
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteDepartmentCommand(id, VersionHeader.Parse(ifMatch)), cancellationToken);
        return result.ToHttpResult(http);
    }
}

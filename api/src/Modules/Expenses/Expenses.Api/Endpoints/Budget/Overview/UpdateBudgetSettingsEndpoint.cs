using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Expenses.Api.Endpoints.Budget.Overview;

internal static class UpdateBudgetSettingsEndpoint
{
    public static RouteGroupBuilder MapUpdateBudgetSettings(this RouteGroupBuilder group)
    {
        group.MapPut("/budget/settings", HandleAsync)
            .WithName("updateBudgetSettings")
            .WithSummary("Sets the monthly limit on all expenses (from the month of monthlyLimitFrom on, left out: the current month), the savings goal and the day limits start carrying overspend. Send the budget version in If-Match.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromHeader(Name = EntryVersionHeader.Name)] string? ifMatch,
        BudgetSettingsRequest request,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateBudgetSettingsCommand(EntryVersionHeader.Parse(ifMatch), request.MonthlyLimit, request.SavingsGoal, request.LimitsFrom, request.MonthlyLimitFrom),
            cancellationToken);
        return result.ToHttpResult(http);
    }
}

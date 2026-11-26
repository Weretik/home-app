using System.Security.Cryptography;
using System.Text;
using BuildingBlocks.Api.Problems;
using BuildingBlocks.Api.RateLimiting;
using Expenses.Application.Features.Imports.Commands.ImportSheet;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Expenses.Api.Endpoints.Import;

/// <summary>
/// <c>POST /api/expenses/imports/sheet</c>: the one-time move from the old Google Sheet. An operator call, not part
/// of the web app, so it is hidden from the OpenAPI document and needs the <c>X-Import-Key</c> header; without a
/// configured key it answers 404. See <c>api/docs/modules/expenses.md</c>.
/// </summary>
internal static class ImportSheetEndpoint
{
    public const string HeaderName = "X-Import-Key";

    public static RouteGroupBuilder MapImportSheet(this RouteGroupBuilder group)
    {
        group.MapPost("/imports/sheet", HandleAsync)
            .ExcludeFromDescription()
            .RequireRateLimiting(RateLimitPolicies.PublicWrite);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        IOptions<SheetImportOptions> options,
        ISender sender,
        HttpContext http,
        CancellationToken cancellationToken)
    {
        var expected = options.Value.Key;
        if (string.IsNullOrEmpty(expected))
            return TypedResults.NotFound();

        var actual = http.Request.Headers[HeaderName].ToString();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(actual), Encoding.UTF8.GetBytes(expected)))
            return TypedResults.Unauthorized();

        var result = await sender.Send(
            new ImportSheetCommand(request.ExpenseRows ?? [], request.IncomeRows ?? [], request.SavingsRows), cancellationToken);
        return result.ToHttpResult(http);
    }

    /// <summary>The tabs as the Google Sheets API returns them: rows of cell text, header first.</summary>
    internal sealed record Request(
        IReadOnlyList<IReadOnlyList<string>>? ExpenseRows,
        IReadOnlyList<IReadOnlyList<string>>? IncomeRows,
        IReadOnlyList<IReadOnlyList<string>>? SavingsRows);
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Expenses.Domain.Budget;
using Expenses.Infrastructure;
using IntegrationTests.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Api.Expenses.Budget;

/// <summary>The month summary's input checks and the budget without its settings row (SDD 003 GAP-012, SDD 004).</summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesSummaryApiTests(ApiFactory api)
{
    [SkippableTheory]
    [InlineData("year=2026&month=13")]
    [InlineData("year=2026&month=0")]
    [InlineData("year=1999&month=5")]
    [InlineData("year=2026")]
    public async Task A_Month_OutsideTheCalendar_IsAValidationProblem(string queryString)
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var response = await client.GetAsync($"/api/expenses/summary?{queryString}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [SkippableFact]
    public async Task Without_TheSettingsRow_TheBudget_IsNotFound_ForReadAndEdit_LikeEachOther()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var budget = await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget");
        var (monthlyLimits, savingsGoal, limitsFrom) = (
            budget.GetProperty("monthlyLimits").GetRawText(),
            budget.GetProperty("savingsGoal").GetDecimal(),
            DateOnly.Parse(budget.GetProperty("limitsFrom").GetString()!, System.Globalization.CultureInfo.InvariantCulture));

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExpensesDbContext>();
        await db.Database.ExecuteSqlAsync($"DELETE FROM expenses.budget_settings");
        try
        {
            var read = await client.GetAsync("/api/expenses/budget");
            using var edit = new HttpRequestMessage(HttpMethod.Put, "/api/expenses/budget/settings")
            {
                Content = JsonContent.Create(new { monthlyLimit = 1m, savingsGoal = 1m }),
            };
            edit.Headers.TryAddWithoutValidation("If-Match", "\"1\"");
            var write = await client.SendAsync(edit);

            Assert.Equal(HttpStatusCode.NotFound, read.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, write.StatusCode);
            Assert.Equal("expenses.budget_settings_not_found", (await read.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
            Assert.Equal("expenses.budget_settings_not_found", (await write.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString());
            // The summary only reads the numbers and goes on without them.
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/expenses/summary?year=2054&month=1")).StatusCode);
        }
        finally
        {
            await db.Database.ExecuteSqlAsync(
                $"INSERT INTO expenses.budget_settings (id, monthly_limits, savings_goal, limits_from) VALUES ({BudgetSettings.SingletonId}, CAST({monthlyLimits} AS jsonb), {savingsGoal}, {limitsFrom})");
        }
    }
}

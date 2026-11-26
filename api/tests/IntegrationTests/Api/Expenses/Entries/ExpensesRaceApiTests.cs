using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Domain.Results;
using Expenses.Application;
using Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;
using Expenses.Application.Features.Expenses.Commands.UpdateExpense;
using Expenses.Application.Features.Planned.Commands.RecordPlannedExpense;
using Expenses.Domain.Budget;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;
using IntegrationTests.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Api.Expenses.Entries;

/// <summary>
/// The race paths: another request changes the same row, or takes the same place, between a handler's read and its
/// save. The handler runs on the real database with <see cref="RacingExpensesDb"/>, which lets the other request in
/// right before the save; the guard must turn the race into a 409 result, never a 500 or a lost update.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesRaceApiTests(ApiFactory api)
{
    private static readonly Guid Groceries = new("86c6ddca-a991-5d9b-97b9-468b90f50c4c");

    [SkippableFact]
    public async Task An_Edit_RacedByAnotherEdit_IsAConflict_AndTheOtherEditStays()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var created = await PostAsync(client, "/api/expenses", new
        {
            date = "2053-01-10", payer = "Shared", categoryId = Groceries, amount = 100m, paymentMethod = "Card", comment = (string?)null,
        });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var version = await VersionAsync(client, "/api/expenses?year=2053&month=1");

        using var scope = api.Services.CreateScope();
        var db = new RacingExpensesDb(scope.ServiceProvider.GetRequiredService<IExpensesDbContext>(), async () =>
            Assert.Equal(HttpStatusCode.NoContent, (await PutAsync(client, $"/api/expenses/{id}", version, new
            {
                date = "2053-01-10", payer = "Maria", categoryId = Groceries, amount = 200m, paymentMethod = "Card", comment = "раньше",
            })).StatusCode));

        var result = await new UpdateExpenseHandler(db).Handle(
            new UpdateExpenseCommand(id, version, new DateOnly(2053, 1, 10), Payer.Vitaliy, Groceries, 300m, PaymentMethod.Cash, null, false),
            CancellationToken.None);

        AssertChangedMeanwhile(result);
        var item = (await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2053&month=1")).GetProperty("items")[0];
        Assert.Equal(200m, item.GetProperty("amount").GetDecimal());
        Assert.Equal("Maria", item.GetProperty("payer").GetString());
    }

    [SkippableFact]
    public async Task A_Record_RacedByAnEditOfThePlan_IsAConflict_AndRecordsNothing()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var plan = new { year = 2053, month = 2, name = "Подарок", categoryId = Groceries, payer = "Shared", amount = 1_000m };
        var created = await PostAsync(client, "/api/expenses/planned", plan);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        var version = await VersionAsync(client, "/api/expenses/planned?year=2053&month=2");

        using var scope = api.Services.CreateScope();
        var db = new RacingExpensesDb(scope.ServiceProvider.GetRequiredService<IExpensesDbContext>(), async () =>
            Assert.Equal(HttpStatusCode.NoContent, (await PutAsync(client, $"/api/expenses/planned/{id}", version, plan with { amount = 1_500m })).StatusCode));

        var result = await new RecordPlannedExpenseHandler(db, TimeProvider.System).Handle(
            new RecordPlannedExpenseCommand(id, version, new DateOnly(2053, 2, 14), PaymentMethod.Card, null),
            CancellationToken.None);

        AssertChangedMeanwhile(result);
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2053&month=2")).GetProperty("totalCount").GetInt32());
        var plans = await client.GetFromJsonAsync<JsonElement>("/api/expenses/planned?year=2053&month=2");
        Assert.Equal(1_500m, plans.GetProperty("totalAmount").GetDecimal());
    }

    [SkippableFact]
    public async Task Two_Limits_AddedAtOnce_DoNotShareAPlace_TheSecondIsAConflict()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        Guid? racer = null;

        using var scope = api.Services.CreateScope();
        var db = new RacingExpensesDb(scope.ServiceProvider.GetRequiredService<IExpensesDbContext>(), async () =>
        {
            var other = await PostAsync(client, "/api/expenses/budget/limits",
                new { name = "Гонка 1", period = "Month", amount = 100m, categoryIds = new[] { Groceries } });
            Assert.Equal(HttpStatusCode.Created, other.StatusCode);
            racer = (await other.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();
        });

        try
        {
            var result = await new CreateSpendingLimitHandler(db, TimeProvider.System).Handle(
                new CreateSpendingLimitCommand(Guid.NewGuid().ToString(), "Гонка 2", LimitPeriod.Month, 100m, [Groceries]),
                CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.Error!.Type);
            Assert.Equal("expenses.limit_added_meanwhile", result.Error.Code);
            var limits = (await client.GetFromJsonAsync<JsonElement>("/api/expenses/budget")).GetProperty("limits");
            Assert.DoesNotContain(limits.EnumerateArray(), l => l.GetProperty("name").GetString() == "Гонка 2");
        }
        finally
        {
            if (racer is { } id)
                await client.DeleteAsync($"/api/expenses/budget/limits/{id}");
        }
    }

    private static void AssertChangedMeanwhile(Result result)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("expenses.changed_meanwhile", result.Error.Code);
    }

    private static async Task<uint> VersionAsync(HttpClient client, string list) =>
        (await client.GetFromJsonAsync<JsonElement>(list)).GetProperty("items")[0].GetProperty("version").GetUInt32();

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string url, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(body) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> PutAsync(HttpClient client, string url, uint version, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, url) { Content = JsonContent.Create(body) };
        request.Headers.TryAddWithoutValidation("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }
}

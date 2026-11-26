using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;

namespace IntegrationTests.Api;

/// <summary>
/// The sheet import and the month lists end to end. Each test uses its own year, so tests sharing the database do
/// not see each other's rows.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ExpensesApiTests(ApiFactory api)
{
    private static readonly string[] ExpenseHeader = ["Дата", "Кто", "Категория", "Сумма, грн", "Оплата", "Комментарий"];
    private static readonly string[] IncomeHeader = ["Дата", "Кто", "Сумма, грн", "Комментарий"];

    [SkippableFact]
    public async Task Categories_ComeFromTheMigration_InSheetOrder()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var categories = await client.GetFromJsonAsync<JsonElement>("/api/expenses/categories");

        Assert.Equal(14, categories.GetArrayLength());
        Assert.Equal("Продукты", categories[0].GetProperty("name").GetString());
    }

    [SkippableFact]
    public async Task The_Import_NeedsTheKey()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var response = await client.PostAsJsonAsync("/api/expenses/imports/sheet", Body("2030", []));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task An_Import_StoresTheRows_AndARepeat_SkipsThem()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        string[][] water = [["21.10.2031", "Мария", "Коммуналка", "17.62", "Карта", "Харківводоканал"]];
        var body = Body("2031", water);

        var first = await ImportAsync(client, body);
        var second = await ImportAsync(client, body);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var report = await first.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(4, report.GetProperty("expensesAdded").GetInt32());
        Assert.Equal(2, report.GetProperty("incomesAdded").GetInt32());
        var repeat = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, repeat.GetProperty("expensesAdded").GetInt32());
        Assert.Equal(4, repeat.GetProperty("expensesSkipped").GetInt32());
        Assert.Equal(2, repeat.GetProperty("incomesSkipped").GetInt32());

        var october = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2031&month=10");
        Assert.Equal(3, october.GetProperty("totalCount").GetInt32());
        Assert.Equal(2424m + 442m + 17.62m, october.GetProperty("totalAmount").GetDecimal());
        Assert.Equal("Коммуналка", october.GetProperty("items")[0].GetProperty("categoryName").GetString());

        var shared = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2031&month=10&payer=Shared");
        Assert.Equal(442m, shared.GetProperty("totalAmount").GetDecimal());

        var incomes = await client.GetFromJsonAsync<JsonElement>("/api/expenses/incomes?year=2031&month=10");
        Assert.Equal(25_025m + 11_640m, incomes.GetProperty("totalAmount").GetDecimal());
    }

    [SkippableFact]
    public async Task Identical_Rows_AreBoth_Imported()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        string[] bill = ["21.10.2032", "Мария", "Коммуналка", "17.62", "Карта", "Харківводоканал"];

        var response = await ImportAsync(client, Body("2032", [bill, bill]));

        var report = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(5, report.GetProperty("expensesAdded").GetInt32());
    }

    [SkippableFact]
    public async Task A_Bad_Row_StopsTheWholeImport_AndNamesTheRow()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        string[][] unknown = [["01.10.2033", "Мария", "Нет такой", "10.00", "Карта", ""]];

        var response = await ImportAsync(client, Body("2033", unknown));
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        var october = await client.GetFromJsonAsync<JsonElement>("/api/expenses?year=2033&month=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("expenses.import_invalid_rows", problem.GetProperty("code").GetString());
        Assert.Contains("Row 5", problem.GetProperty("errors").GetProperty("expenseRows")[0].GetString(), StringComparison.Ordinal);
        Assert.Equal(0, october.GetProperty("totalCount").GetInt32());
    }

    [SkippableFact]
    public async Task A_Month_OutsideTheCalendar_IsAValidationProblem()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var response = await client.GetAsync("/api/expenses?year=2026&month=13");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> ImportAsync(HttpClient client, object body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/expenses/imports/sheet")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("X-Import-Key", ApiFactory.ImportKey);
        return await client.SendAsync(request);
    }

    /// <summary>Three expenses and two incomes of the given year as the sheet shows them, plus extra expense rows.</summary>
    private static object Body(string year, string[][] extraExpenses) => new
    {
        expenseRows = new[]
            {
                ExpenseHeader,
                [$"01.10.{year}", "Мария", "Личные Маша", "2,424.00", "Карта", "Снятие наличных (*6414)"],
                [$"01.10.{year}", "Общее", "Продукты", "442.00", "Карта", "ТМ АГРОМОЛ (*6414)"],
                [$"01.11.{year}", "Виталий", "Личные Маша", "404.00", "Наличные"],
            }
            .Concat(extraExpenses),
        incomeRows = new[]
        {
            IncomeHeader,
            [$"06.10.{year}", "Мария", "25,025.00", "зарплата СТЕП-ІН (*9061)"],
            [$"08.10.{year}", "Виталий", "11,640.00", "від MARHARYTA TSIUPA (*8140)"],
        },
    };
}

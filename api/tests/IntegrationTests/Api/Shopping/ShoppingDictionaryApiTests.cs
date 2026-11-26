using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Shopping.ShoppingRequests;

namespace IntegrationTests.Api.Shopping;

/// <summary>Departments, keywords and learning end to end. Tests create their own departments and put the order back.</summary>
[Collection(ApiCollection.Name)]
public sealed class ShoppingDictionaryApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Dictionary_ComesFromTheMigration()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var departments = (await GetAsync(client, "/departments")).EnumerateArray().ToList();
        var keywords = await GetAsync(client, "/keywords?search=зубн");
        var match = await GetAsync(client, "/departments/match?name=Зубная%20паста");

        Assert.Equal("Овощи и фрукты", departments[0].Text("name"));
        Assert.Equal("🥦", departments[0].Text("icon"));
        var fallback = departments.Single(d => d.GetProperty("isFallback").GetBoolean());
        Assert.Equal(("Другое", "🧺"), (fallback.Text("name"), fallback.Text("icon")));
        Assert.True(departments.Sum(d => d.GetProperty("keywordCount").GetInt32()) >= 396);
        Assert.Contains(keywords.EnumerateArray(), k => k.Text("keyword") == "зубн паст");
        Assert.Equal("Гигиена и аптека", match.Text("departmentName"));
    }

    [SkippableFact]
    public async Task Moving_AnItem_ToAnotherDepartment_LearnsItsName()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var name = $"Штуковина {Unique()}";
        var grocery = await DepartmentAsync(client, "Бакалея");
        var item = (await AddAsync(client, name)).GetProperty("added")[0];
        Assert.Equal("Другое", item.Text("departmentName"));

        var moved = await PutAsync(client, $"/items/{item.Id()}",
            new { name, quantity = (string?)null, note = (string?)null, departmentId = grocery.Id(), urgent = false }, item.Version());
        Assert.Equal(HttpStatusCode.OK, moved.StatusCode);

        var learned = (await GetAsync(client, $"/keywords?search={Uri.EscapeDataString(name)}")).EnumerateArray().Single();
        Assert.Equal(name.ToLowerInvariant(), learned.Text("keyword"));
        Assert.True(learned.GetProperty("learned").GetBoolean());
        Assert.Equal(grocery.Id(), learned.GetProperty("departmentId").GetGuid());
        var match = await GetAsync(client, $"/departments/match?name={Uri.EscapeDataString(name)}");
        Assert.Equal("Бакалея", match.Text("departmentName"));
    }

    [SkippableFact]
    public async Task A_Department_IsAdded_BeforeTheFallback_AndDeleted_IntoIt()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();

        var created = await PostAsync(client, "/departments", new { name = $"Рынок {u}", icon = "🧺" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).Id();
        var duplicate = await PostAsync(client, "/departments", new { name = $"Рынок {u}", icon = "" });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var departments = (await GetAsync(client, "/departments")).EnumerateArray().ToList();
        Assert.True(departments[^1].GetProperty("isFallback").GetBoolean());
        Assert.Equal(id, departments[^2].Id());

        var keyword = await PostAsync(client, "/keywords", new { keyword = $" Рыноч  {u} ", departmentId = id });
        Assert.Equal(HttpStatusCode.Created, keyword.StatusCode);
        var item = (await AddAsync(client, $"творог {u}", departmentId: id)).GetProperty("added")[0];
        Assert.Equal($"Рынок {u}", item.Text("departmentName"));

        var department = departments[^2];
        var renamed = await PutAsync(client, $"/departments/{id}", new { name = $"Базар {u}", icon = "🍎" }, department.Version());
        Assert.Equal(HttpStatusCode.NoContent, renamed.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await DeleteAsync(client, $"/departments/{id}", department.Version())).StatusCode);
        var current = await DepartmentAsync(client, $"Базар {u}");
        Assert.Equal(HttpStatusCode.NoContent, (await DeleteAsync(client, $"/departments/{id}", current.Version())).StatusCode);

        var moved = await InListAsync(client, $"Творог {u}");
        Assert.Equal("Другое", moved!.Value.Text("departmentName"));
        var movedKeyword = (await GetAsync(client, $"/keywords?search=рыноч%20{u}")).EnumerateArray().Single();
        Assert.Equal("Другое", movedKeyword.Text("departmentName"));

        var fallback = await DepartmentAsync(client, "Другое");
        Assert.Equal(HttpStatusCode.Conflict, (await DeleteAsync(client, $"/departments/{fallback.Id()}", fallback.Version())).StatusCode);
    }

    [SkippableFact]
    public async Task The_Order_IsSet_WithEveryDepartmentOnce()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var ids = (await GetAsync(client, "/departments")).EnumerateArray().Select(d => d.Id()).ToList();

        var missing = await PutAsync(client, "/departments/order", new { departmentIds = ids.Skip(1) });
        var reversed = await PutAsync(client, "/departments/order", new { departmentIds = Enumerable.Reverse(ids) });
        var after = (await GetAsync(client, "/departments")).EnumerateArray().Select(d => d.Id()).ToList();
        var restored = await PutAsync(client, "/departments/order", new { departmentIds = ids });

        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, reversed.StatusCode);
        Assert.Equal(Enumerable.Reverse(ids), after);
        Assert.Equal(HttpStatusCode.NoContent, restored.StatusCode);
    }

    [SkippableFact]
    public async Task Keywords_AreNormalised_Unique_AndEditable()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var dairy = await DepartmentAsync(client, "Молочное и яйца");
        var cheese = await DepartmentAsync(client, "Сыр и колбаса");

        var created = await PostAsync(client, "/keywords", new { keyword = $" Сырн  {u} ", departmentId = dairy.Id() });
        var duplicate = await PostAsync(client, "/keywords", new { keyword = $"сырн {u}", departmentId = cheese.Id() });
        var tooShort = await PostAsync(client, "/keywords", new { keyword = "я", departmentId = cheese.Id() });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooShort.StatusCode);

        var keyword = (await GetAsync(client, $"/keywords?departmentId={dairy.Id()}&search={u}")).EnumerateArray().Single();
        Assert.Equal($"сырн {u}", keyword.Text("keyword"));
        Assert.False(keyword.GetProperty("learned").GetBoolean());

        var changed = await PutAsync(client, $"/keywords/{keyword.Id()}", new { keyword = $"сырник {u}", departmentId = cheese.Id() }, keyword.Version());
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        var current = (await GetAsync(client, $"/keywords?search={u}")).EnumerateArray().Single();
        Assert.Equal("Сыр и колбаса", current.Text("departmentName"));
        Assert.Equal(HttpStatusCode.Conflict, (await DeleteAsync(client, $"/keywords/{keyword.Id()}", keyword.Version())).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await DeleteAsync(client, $"/keywords/{keyword.Id()}", current.Version())).StatusCode);
        Assert.Empty((await GetAsync(client, $"/keywords?search={u}")).EnumerateArray());
    }
}

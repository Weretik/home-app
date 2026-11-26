using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingBlocks.Domain.Results;
using IntegrationTests.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shopping.Application;
using Shopping.Application.Features.Departments.Commands.DeleteDepartment;
using Shopping.Application.Features.Items.Commands.UpdateItem;
using Shopping.Domain.Items;
using static IntegrationTests.Api.Shopping.ShoppingRequests;

namespace IntegrationTests.Api.Shopping;

/// <summary>
/// The race paths of the Shopping writes: another request changes the same item, or adds an item to a department,
/// between a handler's read and its save. The handler runs on the real database with <see cref="RacingShoppingDb"/>;
/// the guard must turn the race into a 409 result, never a 500 or a lost update.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class ShoppingRaceApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task An_ItemEdit_RacedByAnotherEdit_IsAConflict_AndTheOtherEditStays()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var item = (await AddAsync(client, $"сметана {u}")).GetProperty("added")[0];
        var department = item.GetProperty("departmentId").GetGuid();

        using var scope = api.Services.CreateScope();
        var db = new RacingShoppingDb(scope.ServiceProvider.GetRequiredService<IShoppingDbContext>(), async () =>
            Assert.Equal(HttpStatusCode.OK, (await PutAsync(client, $"/items/{item.Id()}",
                new { name = $"Сметана {u}", quantity = "2", note = "раньше", departmentId = department, urgent = false }, item.Version())).StatusCode));

        var result = await new UpdateItemHandler(db, TimeProvider.System).Handle(
            new UpdateItemCommand(item.Id(), item.Version(), $"Сметана {u}", "1", "позже", department, true), CancellationToken.None);

        AssertChangedMeanwhile(result);
        var stored = await InListAsync(client, $"Сметана {u}");
        Assert.Equal("раньше", stored!.Value.Text("note"));
        await DeleteAsync(client, $"/items/{item.Id()}", stored.Value.Version());
    }

    [SkippableFact]
    public async Task A_DepartmentDelete_RacedByANewItemInIt_IsAConflict_AndNothingMoves()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var u = Unique();
        var created = await PostAsync(client, "/departments", new { name = $"Гонка {u}", icon = "🧺" });
        var id = (await created.Content.ReadFromJsonAsync<JsonElement>()).Id();
        var version = (await DepartmentAsync(client, $"Гонка {u}")).Version();
        JsonElement? item = null;

        try
        {
            using var scope = api.Services.CreateScope();
            var db = new RacingShoppingDb(scope.ServiceProvider.GetRequiredService<IShoppingDbContext>(), async () =>
                item = (await AddAsync(client, $"ряженка {u}", departmentId: id)).GetProperty("added")[0]);

            var result = await new DeleteDepartmentHandler(db).Handle(new DeleteDepartmentCommand(id, version), CancellationToken.None);

            AssertChangedMeanwhile(result);
            var stored = await InListAsync(client, $"Ряженка {u}");
            Assert.Equal($"Гонка {u}", stored!.Value.Text("departmentName"));
        }
        finally
        {
            if (await InListAsync(client, $"Ряженка {u}") is { } left)
                await DeleteAsync(client, $"/items/{left.Id()}", left.Version());
            await DeleteAsync(client, $"/departments/{id}", (await DepartmentAsync(client, $"Гонка {u}")).Version());
        }
    }

    private static void AssertChangedMeanwhile<T>(Result<T> result)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(ShoppingItemErrors.ChangedMeanwhile.Code, result.Error.Code);
    }

    private static void AssertChangedMeanwhile(Result result)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(ShoppingItemErrors.ChangedMeanwhile.Code, result.Error.Code);
    }
}

using System.Net;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Notes.NoteRequests;

namespace IntegrationTests.Api.Notes;

/// <summary>The list: order, filters and the word search (spec notes/001, R-012, R-013, SC-002, SC-006).</summary>
[Collection(ApiCollection.Name)]
public sealed class NotesListApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_List_IsOrdered_ByCategory_ThenImportant_ThenKind_ThenTitle()
    {
        // R-012: Їжа (10) before Квіти (30), notes without a category last.
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();

        await CreateNoteAsync(client, $"none {tag}");
        await CreateNoteAsync(client, $"flowers {tag}", new { categoryId = Flowers });
        await CreateNoteAsync(client, $"food-fact {tag}", new { kind = "Fact", categoryId = Food });
        await CreateNoteAsync(client, $"Food-b {tag}", new { categoryId = Food });
        await CreateNoteAsync(client, $"food-a {tag}", new { categoryId = Food });
        await CreateNoteAsync(client, $"food-dislike {tag}", new { kind = "Dislikes", categoryId = Food });
        await CreateNoteAsync(client, $"food-important-fact {tag}", new { kind = "Fact", isImportant = true, categoryId = Food });

        var titles = Titles(await ListAsync(client, $"search={tag}")).Select(t => t.Split(' ')[0]);

        Assert.Equal(["food-important-fact", "food-a", "Food-b", "food-dislike", "food-fact", "flowers", "none"], titles);
    }

    [SkippableFact]
    public async Task Filters_Combine_Person_Kinds_AndCategory()
    {
        // R-013, SC-002
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();

        await CreateNoteAsync(client, $"Півонії {tag}", new { categoryId = Flowers });
        await CreateNoteAsync(client, $"Лілії {tag}", new { kind = "Dislikes", categoryId = Flowers });
        await CreateNoteAsync(client, $"Розмір 38 {tag}", new { kind = "Fact" });
        await CreateNoteAsync(client, $"Окрошка {tag}", new { about = "Vitaliy", categoryId = Food });

        Assert.Equal(3, (await ListAsync(client, $"about=Maria&search={tag}")).GetArrayLength());
        Assert.Equal([$"Окрошка {tag}"], Titles(await ListAsync(client, $"about=Vitaliy&search={tag}")));
        Assert.Equal(2, (await ListAsync(client, $"about=Maria&kind=Likes&kind=Fact&search={tag}")).GetArrayLength());
        Assert.Equal(2, (await ListAsync(client, $"categoryId={Flowers}&search={tag}")).GetArrayLength());
        Assert.Equal([$"Розмір 38 {tag}"], Titles(await ListAsync(client, $"withoutCategory=true&search={tag}")));
        Assert.Equal(4, (await ListAsync(client, $"search={tag}")).GetArrayLength());

        var both = await client.GetAsync($"/api/notes/items?categoryId={Flowers}&withoutCategory=true");
        Assert.Equal(HttpStatusCode.BadRequest, both.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/notes/items?about=Somebody")).StatusCode);
    }

    [SkippableFact]
    public async Task Search_FindsEveryWord_InTheTitle_TheDescription_OrThePlace()
    {
        // SC-006
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();

        await CreateNoteAsync(client, $"Молоко {tag}", new { place = "Агромол" });
        await CreateNoteAsync(client, $"Окрошка {tag}", new { about = "Vitaliy", details = "Найкраще влітку, літо без неї не літо" });
        await CreateNoteAsync(client, $"Кава 50% {tag}");

        Assert.Equal([$"Молоко {tag}"], Titles(await ListAsync(client, $"search=АГРОМОЛ%20{tag}")));
        Assert.Equal([$"Окрошка {tag}"], Titles(await ListAsync(client, $"search=окрошка%20літо%20{tag}")));
        Assert.Empty(Titles(await ListAsync(client, $"search=окрошка%20зима%20{tag}")));
        Assert.Equal([$"Кава 50% {tag}"], Titles(await ListAsync(client, $"search=50%25%20{tag}")));
        Assert.Empty(Titles(await ListAsync(client, $"search=_%25%20%25{tag}x")));
    }
}

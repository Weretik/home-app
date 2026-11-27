using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Notes.NoteRequests;

namespace IntegrationTests.Api.Notes;

/// <summary>Notes end to end: create, author from the session, edit, versions, repeats and delete (spec notes/001).</summary>
[Collection(ApiCollection.Name)]
public sealed class NotesItemsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task Vitaliy_WritesAboutMasha_AndTheListSaysWhoWroteIt()
    {
        // SC-001
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();

        var dislike = await CreateNoteAsync(client, $"Молоко {tag}", new { kind = "Dislikes", place = " АТБ ", categoryId = Food, details = "  " });
        await CreateNoteAsync(client, $"Молоко {tag}", new { place = "Агромол", categoryId = Food });

        var list = await ListAsync(client, $"about=Maria&search={tag}");
        Assert.Equal(["Likes", "Dislikes"], list.EnumerateArray().Select(n => n.GetProperty("kind").GetString()));
        var note = list.EnumerateArray().Single(n => n.GetProperty("id").GetGuid() == dislike);
        Assert.Equal("Maria", note.GetProperty("about").GetString());
        Assert.Equal("АТБ", note.GetProperty("place").GetString());
        Assert.Equal(JsonValueKind.Null, note.GetProperty("details").ValueKind);
        Assert.Equal("Їжа", note.GetProperty("categoryName").GetString());
        Assert.Equal("🍲", note.GetProperty("categoryEmoji").GetString());
        Assert.Equal("Vitaliy", note.GetProperty("createdBy").GetString());
        Assert.Equal("Vitaliy", note.GetProperty("updatedBy").GetString());
        Assert.False(note.GetProperty("isImportant").GetBoolean());
        Assert.NotEqual(0u, note.GetProperty("version").GetUInt32());
    }

    [SkippableFact]
    public async Task The_Author_ComesFromTheSession_NotFromTheRequest()
    {
        // R-008, SC-002
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var masha = await MashaAsync(api);
        var tag = Tag();

        var id = await CreateNoteAsync(masha, $"Півонії {tag}", new { isImportant = true, categoryId = Flowers, createdBy = "Vitaliy" });

        var note = await FindAsync(masha, id, tag);
        Assert.Equal("Maria", note.GetProperty("createdBy").GetString());
        Assert.True(note.GetProperty("isImportant").GetBoolean());
        Assert.Equal([$"Півонії {tag}"], Titles(await ListAsync(masha, $"about=Maria&kind=Likes&search={tag}")));
        Assert.Empty(Titles(await ListAsync(masha, $"about=Vitaliy&search={tag}")));
    }

    [SkippableFact]
    public async Task Masha_Edits_VitaliysNote_AndBecomesTheLastEditor()
    {
        // SC-003
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var vitaliy = api.CreateHttpsClient();
        using var masha = await MashaAsync(api);
        var tag = Tag();
        var id = await CreateNoteAsync(vitaliy, $"Томатний сік {tag}", new { about = "Vitaliy", kind = "Dislikes" });
        var version = (await FindAsync(masha, id, tag)).GetProperty("version").GetUInt32();

        var edited = await SendAsync(masha, HttpMethod.Put, $"/api/notes/items/{id}", version,
            Body($"Томатний сік {tag}", new { about = "Vitaliy", kind = "Dislikes", details = "навіть з сіллю", isImportant = true }));

        Assert.Equal(HttpStatusCode.NoContent, edited.StatusCode);
        var note = await FindAsync(vitaliy, id, tag);
        Assert.Equal("Vitaliy", note.GetProperty("createdBy").GetString());
        Assert.Equal("Maria", note.GetProperty("updatedBy").GetString());
        Assert.Equal("навіть з сіллю", note.GetProperty("details").GetString());
        Assert.NotEqual(version, note.GetProperty("version").GetUInt32());
    }

    [SkippableFact]
    public async Task A_Stale_Version_AnswersConflict_ForAnEdit_AndADelete()
    {
        // R-009, SC-004
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();
        var id = await CreateNoteAsync(client, $"Окрошка {tag}", new { about = "Vitaliy" });
        var version = (await FindAsync(client, id, tag)).GetProperty("version").GetUInt32();

        var first = await SendAsync(client, HttpMethod.Put, $"/api/notes/items/{id}", version, Body($"Окрошка на кефірі {tag}", new { about = "Vitaliy" }));
        var second = await SendAsync(client, HttpMethod.Put, $"/api/notes/items/{id}", version, Body($"Окрошка на квасі {tag}", new { about = "Vitaliy" }));
        var staleDelete = await SendAsync(client, HttpMethod.Delete, $"/api/notes/items/{id}", version);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("notes.changed_meanwhile", await CodeAsync(second));
        Assert.Equal(HttpStatusCode.Conflict, staleDelete.StatusCode);
        Assert.Equal("notes.changed_meanwhile", await CodeAsync(staleDelete));

        var current = (await FindAsync(client, id, tag)).GetProperty("version").GetUInt32();
        Assert.Equal(HttpStatusCode.NoContent, (await SendAsync(client, HttpMethod.Delete, $"/api/notes/items/{id}", current)).StatusCode);
        Assert.Empty(Titles(await ListAsync(client, $"search={tag}")));
        var missing = await SendAsync(client, HttpMethod.Delete, $"/api/notes/items/{id}", current);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("notes.note_not_found", await CodeAsync(missing));
        var missingEdit = await SendAsync(client, HttpMethod.Put, $"/api/notes/items/{id}", current, Body($"Окрошка {tag}"));
        Assert.Equal("notes.note_not_found", await CodeAsync(missingEdit));
    }

    [SkippableFact]
    public async Task A_Repeat_ReturnsTheFirstNote_AndTheSameKey_WithOtherData_IsAConflict()
    {
        // R-010
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var tag = Tag();
        var key = Guid.NewGuid().ToString();

        var first = await PostAsync(client, "/api/notes/items", Body($"Технології {tag}", new { about = "Vitaliy" }), key);
        var repeat = await PostAsync(client, "/api/notes/items", Body($"Технології {tag}", new { about = "Vitaliy" }), key);
        var other = await PostAsync(client, "/api/notes/items", Body($"Гаджети {tag}", new { about = "Vitaliy" }), key);
        var noKey = await PostAsync(client, "/api/notes/items", Body($"Без ключа {tag}"));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, repeat.StatusCode);
        Assert.Equal(
            (await first.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid(),
            (await repeat.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid());
        Assert.Equal(HttpStatusCode.Conflict, other.StatusCode);
        Assert.Equal("notes.idempotency_key_reused", await CodeAsync(other));
        Assert.Equal(HttpStatusCode.BadRequest, noKey.StatusCode);
        Assert.Single(Titles(await ListAsync(client, $"search={tag}")));
    }

    [SkippableFact]
    public async Task Bad_Input_IsRefused_WithUkrainianProblems()
    {
        // R-003, R-006
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();

        var empty = await PostAsync(client, "/api/notes/items", Body("   "), Guid.NewGuid().ToString());
        var unknownCategory = await PostAsync(client, "/api/notes/items", Body($"Чай {Tag()}", new { categoryId = Guid.NewGuid() }), Guid.NewGuid().ToString());
        var unknownKind = await PostAsync(client, "/api/notes/items", Body($"Чай {Tag()}", new { kind = "Loves" }), Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, unknownCategory.StatusCode);
        Assert.Equal("notes.category_not_found", await CodeAsync(unknownCategory));
        Assert.Equal(HttpStatusCode.BadRequest, unknownKind.StatusCode);
    }
}

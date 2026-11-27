using IntegrationTests.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notes.Infrastructure;

namespace IntegrationTests.Api.Notes;

/// <summary>The schema the Initial migration creates: the unique index in SQL, the foreign key that clears categories, no extension.</summary>
[Collection(ApiCollection.Name)]
public sealed class NotesSchemaTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Migration_Creates_TheUniqueNameIndex_AndTheListIndexes()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NotesDbContext>();

        var indexes = await db.Database
            .SqlQueryRaw<string>("SELECT indexdef AS \"Value\" FROM pg_indexes WHERE schemaname = 'notes'")
            .ToListAsync();
        var foreignKey = await db.Database
            .SqlQueryRaw<string>("SELECT pg_get_constraintdef(oid) AS \"Value\" FROM pg_constraint WHERE conname = 'fk_notes_categories'")
            .SingleAsync();

        Assert.Contains(indexes, i => i.StartsWith("CREATE UNIQUE INDEX ux_categories_lower_name", StringComparison.Ordinal) && i.Contains("lower", StringComparison.Ordinal));
        Assert.Contains(indexes, i => i.Contains("ix_notes_about_kind", StringComparison.Ordinal));
        Assert.Contains(indexes, i => i.Contains("ix_notes_category_id", StringComparison.Ordinal));
        Assert.Contains("ON DELETE SET NULL", foreignKey, StringComparison.Ordinal);
    }
}

using IntegrationTests.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Todos.Infrastructure;

namespace IntegrationTests.Api.Todos;

/// <summary>The indexes the Initial migration creates in SQL, which the EF model does not know about.</summary>
[Collection(ApiCollection.Name)]
public sealed class TodosSchemaTests(ApiFactory api)
{
    [SkippableFact]
    public async Task The_Migration_Creates_TheSearchIndex_AndTheUniqueIndexes()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TodosDbContext>();

        var indexes = await db.Database
            .SqlQueryRaw<string>("SELECT indexdef AS \"Value\" FROM pg_indexes WHERE schemaname = 'todos'")
            .ToListAsync();

        Assert.Contains(indexes, i => i.Contains("ix_tasks_lower_title_trgm", StringComparison.Ordinal) && i.Contains("gin_trgm_ops", StringComparison.Ordinal));
        Assert.Contains(indexes, i => i.StartsWith("CREATE UNIQUE INDEX ux_categories_lower_name", StringComparison.Ordinal) && i.Contains("lower", StringComparison.Ordinal));
        Assert.Contains(indexes, i => i.StartsWith("CREATE UNIQUE INDEX ux_tasks_import_key", StringComparison.Ordinal) && !i.Contains("WHERE", StringComparison.Ordinal));
    }
}

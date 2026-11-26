using Microsoft.EntityFrameworkCore;

namespace ArchitectureTests.Boundaries;

/// <summary>
/// Each module owns its data: its DbContext maps only the module's own types, all in the module's schema (so no
/// foreign key can reach another schema), and the committed migrations match the model. Built from the model alone,
/// without a database.
/// </summary>
public sealed class DataOwnershipTests
{
    public static TheoryData<string> Modules => new(Repository.Modules);

    private static DbContext ContextOf(string module)
    {
        var type = Repository.Assembly($"{module}.Infrastructure").GetTypes()
            .Single(t => !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t));
        var builder = (DbContextOptionsBuilder)Activator.CreateInstance(typeof(DbContextOptionsBuilder<>).MakeGenericType(type))!;
        builder.UseNpgsql("Host=localhost", npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", module.ToLowerInvariant()));
        return (DbContext)Activator.CreateInstance(type, builder.Options)!;
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void A_Module_MapsOnlyItsOwnTypes_InItsOwnSchema(string module)
    {
        using var context = ContextOf(module);
        var schema = module.ToLowerInvariant();

        var violations = context.Model.GetEntityTypes()
            .Where(e => e.GetSchema() != schema || !e.ClrType.Namespace!.StartsWith($"{module}.", StringComparison.Ordinal))
            .Select(e => $"{e.ClrType.FullName} in schema {e.GetSchema()}")
            .Concat(context.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys())
                .Where(fk => fk.PrincipalEntityType.GetSchema() != schema)
                .Select(fk => $"{fk.DeclaringEntityType.ClrType.Name} -> {fk.PrincipalEntityType.ClrType.Name}"));

        Assert.Empty(violations);
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void The_Migrations_MatchTheModel(string module)
    {
        using var context = ContextOf(module);

        Assert.False(context.Database.HasPendingModelChanges(), $"{module}: the model changed without a migration");
    }
}

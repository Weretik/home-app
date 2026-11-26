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
            .Where(e => e.GetSchema() != schema || !IsOwnType(module, e.ClrType))
            .Select(e => $"{e.ClrType.FullName} in schema {e.GetSchema()}")
            .Concat(context.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys())
                .Where(fk => fk.PrincipalEntityType.GetSchema() != schema)
                .Select(fk => $"{fk.DeclaringEntityType.ClrType.Name} -> {fk.PrincipalEntityType.ClrType.Name}"));

        Assert.Empty(violations);
    }

    /// <summary>
    /// The module's own types, plus the shared outbox table that a publishing module maps into its own schema
    /// (ADR 0035).
    /// </summary>
    private static bool IsOwnType(string module, Type type) =>
        type.Namespace!.StartsWith($"{module}.", StringComparison.Ordinal)
        || type.FullName == "BuildingBlocks.Infrastructure.Outbox.Storage.OutboxMessage";

    [Theory]
    [MemberData(nameof(Modules))]
    public void The_Migrations_MatchTheModel(string module)
    {
        using var context = ContextOf(module);

        Assert.False(context.Database.HasPendingModelChanges(), $"{module}: the model changed without a migration");
    }
}

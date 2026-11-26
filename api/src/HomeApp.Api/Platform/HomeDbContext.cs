using Microsoft.EntityFrameworkCore;

namespace HomeApp.Api.Platform;

/// <summary>
/// The one database context. Everything lives in the <c>home</c> schema, including the migrations history, because
/// the PostgreSQL server is shared with another project. Features add their tables with <c>DbSet</c> properties and
/// <c>IEntityTypeConfiguration</c> classes next to the feature.
/// </summary>
public sealed class HomeDbContext(DbContextOptions<HomeDbContext> options) : DbContext(options)
{
    public const string Schema = "home";
    public const string MigrationsHistoryTable = "__ef_migrations_history";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HomeDbContext).Assembly);
    }
}

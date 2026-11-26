using Microsoft.EntityFrameworkCore;

namespace HomeApp.Api.Platform;

internal static class DatabaseSetup
{
    /// <summary>Registers <see cref="HomeDbContext"/> only when <c>ConnectionStrings:Default</c> is set.</summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            return services;

        return services.AddDbContext<HomeDbContext>(options => Configure(options, connectionString));
    }

    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsHistoryTable(HomeDbContext.MigrationsHistoryTable, HomeDbContext.Schema));
}

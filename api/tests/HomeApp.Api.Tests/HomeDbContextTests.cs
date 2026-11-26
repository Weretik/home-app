using HomeApp.Api.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace HomeApp.Api.Tests;

public sealed class HomeDbContextTests
{
    [Fact]
    public void Tables_and_migrations_history_live_in_the_home_schema()
    {
        var options = new DbContextOptionsBuilder<HomeDbContext>();
        DatabaseSetup.Configure(options, "Host=localhost;Database=test");
        using var context = new HomeDbContext(options.Options);

        Assert.Equal("home", context.Model.GetDefaultSchema());
        var history = context.GetService<IHistoryRepository>();
        var script = history.GetCreateScript();
        Assert.Contains("home.__ef_migrations_history", script);
    }
}

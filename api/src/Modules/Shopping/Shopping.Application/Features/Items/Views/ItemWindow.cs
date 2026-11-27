using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;
using Shopping.Domain.Settings;

namespace Shopping.Application.Features.Items.Views;

/// <summary>"Now", the module settings and the cart cutoff that follows from them, read once per request.</summary>
public sealed record ItemWindow(DateTimeOffset Now, int CartKeepHours, int FrequentDays)
{
    public DateTimeOffset Cutoff => CartWindow.Cutoff(Now, CartKeepHours);

    public DateTimeOffset FrequentSince => Now.AddDays(-FrequentDays);

    public static async Task<ItemWindow> ReadAsync(IShoppingDbContext db, TimeProvider clock, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking().FirstOrDefaultAsync(cancellationToken) ?? ShoppingSettings.Default();
        // PostgreSQL keeps microseconds: cut "now" to them, so an answer built from a saved entity equals a later read.
        var now = clock.GetUtcNow();
        now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMicrosecond));
        return new ItemWindow(now, settings.CartKeepHours, settings.FrequentDays);
    }
}

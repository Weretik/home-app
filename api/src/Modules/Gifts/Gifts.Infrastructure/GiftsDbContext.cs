using BuildingBlocks.Infrastructure.Persistence;
using Gifts.Application;
using Gifts.Application.Idempotency;
using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Gifts.Infrastructure.Persistence.Wishlist;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Infrastructure;

/// <summary>The module's tables, all in the <c>gifts</c> schema, with their own migration history.</summary>
public sealed class GiftsDbContext(DbContextOptions<GiftsDbContext> options)
    : ModuleDbContext(options, Schema), IGiftsDbContext
{
    public const string Schema = "gifts";

    /// <summary>The purchase events for Todos go out through the module's outbox (ADR 0035).</summary>
    protected override bool HasOutbox => true;

    public DbSet<Gift> Gifts => Set<Gift>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Detail> Details => Set<Detail>();

    public DbSet<CreateRequest> CreateRequests => Set<CreateRequest>();

    /// <summary>
    /// A gift whose variants were replaced is saved as changed even when its own columns are the same, so its row
    /// version (<c>xmin</c>) is checked and moves: the variants are part of the gift (ADR 0022).
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.DetectChanges();
        var touched = ChangeTracker.Entries<GiftVariant>()
            .Where(v => v.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            .Select(v => v.Property<Guid>(GiftVariantColumns.GiftId).CurrentValue)
            .ToHashSet();
        foreach (var gift in ChangeTracker.Entries<Gift>().Where(g => g.State == EntityState.Unchanged && touched.Contains(g.Entity.Id)))
            gift.Property(g => g.Name).IsModified = true;

        return base.SaveChangesAsync(cancellationToken);
    }
}

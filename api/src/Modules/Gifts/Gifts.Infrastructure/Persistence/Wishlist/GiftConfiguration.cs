using Gifts.Domain.Categories;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gifts.Infrastructure.Persistence.Wishlist;

/// <summary>
/// <c>gifts.gifts</c> and its owned <c>gifts.gift_variants</c>. The names are fixed: the one-time import SQL writes
/// into them (ADR 0026).
/// </summary>
internal sealed class GiftConfiguration : IEntityTypeConfiguration<Gift>
{
    private const int EnumLength = 16;

    public void Configure(EntityTypeBuilder<Gift> builder)
    {
        builder.ToTable("gifts");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(g => g.Recipient).HasColumnName("recipient").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(g => g.Name).HasColumnName("name").HasMaxLength(Gift.MaxNameLength).IsRequired();
        builder.Property(g => g.CategoryId).HasColumnName("category_id");
        builder.Property(g => g.Priority).HasColumnName("priority").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(g => g.Price).HasColumnName("price").HasPrecision(12, 2);
        builder.Property(g => g.Link).HasColumnName("link").HasMaxLength(Gift.MaxLinkLength);
        builder.Property(g => g.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(g => g.GiftedOn).HasColumnName("gifted_on");
        builder.Property(g => g.Occasion).HasColumnName("occasion").HasMaxLength(Gift.MaxOccasionLength);
        builder.Property(g => g.OccasionDate).HasColumnName("occasion_date");
        builder.Property(g => g.Notes).HasColumnName("notes").HasMaxLength(Gift.MaxNotesLength);
        builder.Property(g => g.Giver).HasColumnName("giver").HasConversion<string>().HasMaxLength(EnumLength);
        builder.Property(g => g.BuyBy).HasColumnName("buy_by");
        builder.Property(g => g.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(g => g.Version).IsRowVersion();

        // Same schema, so a real foreign key: a category with gifts cannot disappear.
        builder.HasOne<Category>().WithMany().HasForeignKey(g => g.CategoryId).OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_gifts_categories");
        builder.HasIndex(g => g.CategoryId).HasDatabaseName("ix_gifts_category_id");
        builder.HasIndex(g => g.Status).HasDatabaseName("ix_gifts_status");

        builder.Navigation(g => g.Variants).HasField("_variants").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.OwnsMany(g => g.Variants, variants =>
        {
            variants.ToTable("gift_variants");
            variants.WithOwner().HasForeignKey(GiftVariantColumns.GiftId).HasConstraintName("fk_gift_variants_gifts");
            variants.HasKey(v => v.Id);
            variants.Property(v => v.Id).HasColumnName("id").ValueGeneratedNever();
            variants.Property<Guid>(GiftVariantColumns.GiftId).HasColumnName("gift_id");
            variants.Property(v => v.Title).HasColumnName("title").HasMaxLength(Gift.MaxNameLength);
            variants.Property(v => v.Link).HasColumnName("link").HasMaxLength(Gift.MaxLinkLength).IsRequired();
            variants.Property(v => v.Price).HasColumnName("price").HasPrecision(12, 2);
            variants.Property(v => v.SortOrder).HasColumnName("sort_order");
            variants.HasIndex(GiftVariantColumns.GiftId).HasDatabaseName("ix_gift_variants_gift_id");
        });
    }
}

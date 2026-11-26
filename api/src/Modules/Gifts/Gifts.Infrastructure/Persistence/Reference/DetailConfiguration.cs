using Gifts.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gifts.Infrastructure.Persistence.Reference;

/// <summary><c>gifts.details</c>: the «нюансы» per person. The names are fixed: the import SQL writes into them.</summary>
internal sealed class DetailConfiguration : IEntityTypeConfiguration<Detail>
{
    public void Configure(EntityTypeBuilder<Detail> builder)
    {
        builder.ToTable("details");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.Recipient).HasColumnName("recipient").HasConversion<string>().HasMaxLength(16);
        builder.Property(d => d.Label).HasColumnName("label").HasMaxLength(Detail.MaxLabelLength).IsRequired();
        builder.Property(d => d.Value).HasColumnName("value").HasMaxLength(Detail.MaxValueLength).IsRequired();
        builder.Property(d => d.SortOrder).HasColumnName("sort_order");
        builder.Property(d => d.Version).IsRowVersion();
        builder.HasIndex(d => new { d.Recipient, d.SortOrder }).HasDatabaseName("ix_details_recipient_sort_order");
    }
}

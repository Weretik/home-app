using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopping.Domain.Settings;

namespace Shopping.Infrastructure.Persistence.Reference;

internal sealed class ShoppingSettingsConfiguration : IEntityTypeConfiguration<ShoppingSettings>
{
    public void Configure(EntityTypeBuilder<ShoppingSettings> builder)
    {
        builder.ToTable("shopping_settings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.CartKeepHours).HasColumnName("cart_keep_hours");
        builder.Property(s => s.FrequentDays).HasColumnName("frequent_days");
        builder.Property(s => s.Version).IsRowVersion();

        builder.HasData(new
        {
            Id = ShoppingSettings.SingletonId,
            CartKeepHours = ShoppingSettings.DefaultCartKeepHours,
            FrequentDays = ShoppingSettings.DefaultFrequentDays,
        });
    }
}

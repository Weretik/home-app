using Shopping.Domain.Settings;

namespace UnitTests.Shopping.Domain;

public sealed class ShoppingSettingsTests
{
    [Fact]
    public void The_Defaults_AreTheBots_3Hours_And183Days()
    {
        var settings = ShoppingSettings.Default();

        Assert.Equal(ShoppingSettings.SingletonId, settings.Id);
        Assert.Equal(3, settings.CartKeepHours);
        Assert.Equal(183, settings.FrequentDays);
    }

    [Theory]
    [InlineData(1, 30)]
    [InlineData(72, 730)]
    public void Values_InRange_AreStored(int hours, int days)
    {
        var settings = ShoppingSettings.Default();

        Assert.True(settings.Update(hours, days).IsSuccess);
        Assert.Equal(hours, settings.CartKeepHours);
        Assert.Equal(days, settings.FrequentDays);
    }

    [Theory]
    [InlineData(0, 183, "shopping.cart_keep_hours_out_of_range")]
    [InlineData(73, 183, "shopping.cart_keep_hours_out_of_range")]
    [InlineData(3, 29, "shopping.frequent_days_out_of_range")]
    [InlineData(3, 731, "shopping.frequent_days_out_of_range")]
    public void Values_OutOfRange_AreRefused_AndTheOldOnes_Stay(int hours, int days, string error)
    {
        var settings = ShoppingSettings.Default();

        Assert.Equal(error, settings.Update(hours, days).Error!.Code);
        Assert.Equal(3, settings.CartKeepHours);
        Assert.Equal(183, settings.FrequentDays);
    }
}

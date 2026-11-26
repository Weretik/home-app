using Notifications.Infrastructure.Telegram;

namespace UnitTests.Notifications.Planning;

public sealed class TelegramWebhookSecretTests
{
    [Fact]
    public void The_Secret_IsDerivedFromTheToken_AndMatchesOnlyItself()
    {
        var secret = TelegramWebhookSecret.For("123:abc");

        Assert.Equal(32, secret.Length);
        Assert.Equal(secret, TelegramWebhookSecret.For("123:abc"));
        Assert.NotEqual(secret, TelegramWebhookSecret.For("123:abd"));
        Assert.True(TelegramWebhookSecret.Matches("123:abc", secret));
        Assert.False(TelegramWebhookSecret.Matches("123:abc", null));
        Assert.False(TelegramWebhookSecret.Matches(string.Empty, TelegramWebhookSecret.For(string.Empty)));
    }
}

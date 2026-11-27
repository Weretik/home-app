using Notifications.Infrastructure.Telegram;

namespace UnitTests.Notifications.Telegram;

public sealed class TelegramWebhookSecretTests
{
    [Fact]
    public void The_Secret_IsDerivedFromTheToken_AndMatchesOnlyItself()
    {
        var secret = TelegramWebhookSecret.For("7712345678:AAHk-unit_Token0123456789abcdefgh");

        Assert.Equal(32, secret.Length);
        Assert.Equal(secret, TelegramWebhookSecret.For("7712345678:AAHk-unit_Token0123456789abcdefgh"));
        Assert.NotEqual(secret, TelegramWebhookSecret.For("7712345678:AAHk-unit_Token0123456789abcdefgi"));
        Assert.True(TelegramWebhookSecret.Matches("7712345678:AAHk-unit_Token0123456789abcdefgh", secret));
        Assert.False(TelegramWebhookSecret.Matches("7712345678:AAHk-unit_Token0123456789abcdefgh", null));
        Assert.False(TelegramWebhookSecret.Matches(string.Empty, TelegramWebhookSecret.For(string.Empty)));
    }
}

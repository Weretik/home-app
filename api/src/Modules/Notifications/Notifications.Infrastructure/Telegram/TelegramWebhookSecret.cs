using System.Security.Cryptography;
using System.Text;

namespace Notifications.Infrastructure.Telegram;

/// <summary>
/// The <c>secret_token</c> Telegram sends with every webhook call, derived from the bot token so there is no second
/// secret to keep (ADR 0033): the first 32 hex characters of SHA-256 of <c>webhook:&lt;token&gt;</c>.
/// </summary>
public static class TelegramWebhookSecret
{
    public const string HeaderName = "X-Telegram-Bot-Api-Secret-Token";

    public static string For(string botToken) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes("webhook:" + botToken)))[..32];

    /// <summary>Compares in constant time; false when no token is set.</summary>
    public static bool Matches(string botToken, string? received) =>
        !string.IsNullOrEmpty(botToken)
        && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(For(botToken)), Encoding.UTF8.GetBytes(received ?? string.Empty));
}

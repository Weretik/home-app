using BuildingBlocks.Domain.Modeling;
using Notifications.Domain.Chats;

namespace Notifications.Domain.Deliveries;

/// <summary>
/// One planned message for one <see cref="Channel"/>. Its <see cref="Key"/> (<c>&lt;kind&gt;:&lt;period&gt;:&lt;audience&gt;</c>,
/// with <c>push:</c> in front for push) is unique, so a repeated or parallel tick cannot plan the same message twice
/// (R-007, R-106). The outbox sends it and sets <see cref="SentAtUtc"/>.
/// </summary>
public sealed class Delivery : Entity<DeliveryId>, IAggregateRoot
{
    public const int MaxKeyLength = 200;

    private Delivery()
    {
    }

    public string Key { get; private set; } = string.Empty;

    public DigestKind Kind { get; private set; }

    public DeliveryChannel Channel { get; private set; }

    public Audience Audience { get; private set; }

    /// <summary>The Telegram chat; 0 for push, which goes to every device of the audience.</summary>
    public long ChatId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <summary>When Telegram or a push service accepted it; null while it waits in the outbox.</summary>
    public DateTimeOffset? SentAtUtc { get; private set; }

    public static Delivery Plan(
        string key, DeliveryChannel channel, DigestKind kind, Audience audience, long chatId, DateTimeOffset now) => new()
    {
        Id = DeliveryId.New(now),
        Key = key,
        Channel = channel,
        Kind = kind,
        Audience = audience,
        ChatId = chatId,
        CreatedAtUtc = now,
    };

    /// <summary>The unique key of a message of <paramref name="kind"/> for <paramref name="period"/> and <paramref name="audience"/>.</summary>
    public static string KeyOf(DigestKind kind, string period, Audience audience) => $"{kind}:{period}:{audience}";

    /// <summary>The key of the same message on <paramref name="channel"/>: Telegram keys stay as they were, push gets <c>push:</c>.</summary>
    public static string KeyOf(DeliveryChannel channel, string telegramKey) =>
        channel == DeliveryChannel.Push ? $"push:{telegramKey}" : telegramKey;
}

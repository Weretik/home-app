using BuildingBlocks.Domain.Modeling;
using Notifications.Domain.Chats;

namespace Notifications.Domain.Deliveries;

/// <summary>
/// One planned message. Its <see cref="Key"/> (<c>&lt;kind&gt;:&lt;period&gt;:&lt;audience&gt;</c>) is unique, so a
/// repeated or parallel tick cannot plan the same message twice (R-007). The outbox sends it and sets
/// <see cref="SentAtUtc"/>.
/// </summary>
public sealed class Delivery : Entity<Guid>, IAggregateRoot
{
    public const int MaxKeyLength = 200;

    private Delivery()
    {
    }

    public string Key { get; private set; } = string.Empty;

    public DigestKind Kind { get; private set; }

    public Audience Audience { get; private set; }

    public long ChatId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <summary>When Telegram accepted it; null while it waits in the outbox.</summary>
    public DateTimeOffset? SentAtUtc { get; private set; }

    public static Delivery Plan(string key, DigestKind kind, Audience audience, long chatId, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(now),
        Key = key,
        Kind = kind,
        Audience = audience,
        ChatId = chatId,
        CreatedAtUtc = now,
    };

    /// <summary>The unique key of a message of <paramref name="kind"/> for <paramref name="period"/> and <paramref name="audience"/>.</summary>
    public static string KeyOf(DigestKind kind, string period, Audience audience) => $"{kind}:{period}:{audience}";
}

using BuildingBlocks.Domain.Modeling;

namespace Notifications.Domain.Chats;

/// <summary>
/// A Telegram chat the bot writes to for one <see cref="Audience"/>. Linked from Telegram (<c>/start</c> and a tap,
/// or adding the bot to a group) and unlinked on the site; unique indexes keep one chat per audience and one audience
/// per chat (ADR 0036).
/// </summary>
public sealed class LinkedChat : Entity<LinkedChatId>, IAggregateRoot
{
    public const int MaxTitleLength = 200;

    private LinkedChat()
    {
    }

    public Audience Audience { get; private set; }

    /// <summary>Telegram's chat id: positive for a private chat, negative for a group.</summary>
    public long ChatId { get; private set; }

    /// <summary>The person's name or the group's title as Telegram reported it when linking.</summary>
    public string? Title { get; private set; }

    public DateTimeOffset LinkedAtUtc { get; private init; }

    public static LinkedChat Link(Audience audience, long chatId, string? title, DateTimeOffset now) => new()
    {
        Id = LinkedChatId.New(now),
        Audience = audience,
        ChatId = chatId,
        Title = Shorten(title),
        LinkedAtUtc = now,
    };

    private static string? Shorten(string? title)
    {
        var trimmed = title?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= MaxTitleLength ? trimmed : trimmed[..MaxTitleLength];
    }
}

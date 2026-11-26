using BuildingBlocks.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;

/// <summary>
/// Links and unlinks chats from Telegram (ADR 0036): a free slot is taken by the first chat that asks; a slot that
/// holds another chat, or a chat that already serves another audience, is refused. The unique indexes decide a race.
/// </summary>
public static class ChatLinking
{
    /// <summary>
    /// Links <paramref name="chatId"/> for <paramref name="audience"/>. Returns null when it is linked now (or was
    /// already), else the audience that blocks it: the chat's own other audience, or the audience whose slot is taken.
    /// </summary>
    public static async Task<(bool Linked, Audience? Current)> LinkAsync(
        INotificationsDbContext db, Audience audience, long chatId, string? title, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var holders = await db.Chats.AsNoTracking()
            .Where(c => c.Audience == audience || c.ChatId == chatId)
            .Select(c => new { c.Audience, c.ChatId })
            .ToListAsync(cancellationToken);
        if (holders.FirstOrDefault(c => c.ChatId == chatId) is { } own)
            return (own.Audience == audience, own.Audience);
        if (holders.Count > 0)
            return (false, null);

        db.Chats.Add(LinkedChat.Link(audience, chatId, title, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return (true, null);
        }
        catch (UniqueConstraintViolationException)
        {
            return (false, null);
        }
    }

    /// <summary>The bot left or was removed from the chat: it can no longer write there.</summary>
    public static Task UnlinkAsync(INotificationsDbContext db, long chatId, CancellationToken cancellationToken) =>
        db.Chats.Where(c => c.ChatId == chatId).ExecuteDeleteAsync(cancellationToken);
}

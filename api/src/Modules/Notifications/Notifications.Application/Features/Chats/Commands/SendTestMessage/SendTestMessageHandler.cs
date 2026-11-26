using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Digests.Texts;
using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Chats.Commands.SendTestMessage;

/// <summary>
/// Sends at once, not through the outbox: the person on the site waits for the answer, and nothing is stored
/// (ADR 0033). Telegram's refusal is reported as 409.
/// </summary>
public sealed class SendTestMessageHandler(INotificationsDbContext db, ITelegramBot bot) : ICommandHandler<SendTestMessageCommand, Result>
{
    public async ValueTask<Result> Handle(SendTestMessageCommand c, CancellationToken cancellationToken)
    {
        if (!bot.IsConfigured)
            return ChatErrors.NotConfigured;

        var chatId = await db.Chats.AsNoTracking().Where(x => x.Audience == c.Audience).Select(x => (long?)x.ChatId)
            .FirstOrDefaultAsync(cancellationToken);
        if (chatId is null)
            return ChatErrors.NotLinked(c.Audience);

        var text = $"✅ Бот на связи. Этот чат подключён: {TextFormat.Person(c.Audience.ToString())}.";
        return await bot.TrySendAsync(chatId.Value, text, [], cancellationToken) ? Result.Success() : ChatErrors.TelegramFailed;
    }
}

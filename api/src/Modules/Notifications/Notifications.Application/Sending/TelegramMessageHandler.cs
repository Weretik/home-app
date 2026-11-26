using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Application.Sending;

/// <summary>
/// Sends a planned message and records when Telegram accepted it. A refusal throws, and the outbox retries with
/// growing pauses. A repeat after the send but before the record sends the message again (at least once, R-007).
/// </summary>
public sealed class TelegramMessageHandler(INotificationsDbContext db, ITelegramBot bot, TimeProvider clock)
    : IOutboxMessageHandler<TelegramMessage>
{
    public async Task HandleAsync(TelegramMessage message, CancellationToken cancellationToken)
    {
        await bot.SendAsync(message.ChatId, message.Text, message.Buttons, cancellationToken);
        var now = clock.GetUtcNow();
        await db.Deliveries
            .Where(d => d.Id == message.DeliveryId)
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.SentAtUtc, (DateTimeOffset?)now), cancellationToken);
    }
}

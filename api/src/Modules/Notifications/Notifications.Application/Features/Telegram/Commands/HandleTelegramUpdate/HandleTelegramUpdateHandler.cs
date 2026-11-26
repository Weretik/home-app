using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notifications.Application.Digests.Planning;
using Notifications.Application.Digests.Texts;
using Notifications.Application.Sending;
using Notifications.Domain.Chats;
using Todos.Contracts;

namespace Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;

/// <summary>
/// The bot's answers: linking chats, <c>/today</c>, <c>/help</c> and the «✅» buttons. Answers go out at once and best
/// effort (ADR 0033); a failed answer is logged by the adapter and the person asks again. Always succeeds, so the
/// webhook answers 200 and Telegram does not resend the update.
/// </summary>
public sealed class HandleTelegramUpdateHandler(
    INotificationsDbContext db,
    ITelegramBot bot,
    ITodosDigest todos,
    ITodoCompletion completion,
    NotificationsSettings site,
    TimeProvider clock) : ICommandHandler<HandleTelegramUpdateCommand, Result>
{
    private static readonly IReadOnlyList<TelegramButton> NoButtons = [];

    public async ValueTask<Result> Handle(HandleTelegramUpdateCommand c, CancellationToken cancellationToken)
    {
        if (c.MemberStatus is { } status)
            await OnMembershipAsync(c, status, cancellationToken);
        else if (c.CallbackId is { } callbackId)
            await OnButtonAsync(c, callbackId, c.CallbackData ?? string.Empty, cancellationToken);
        else if (Command(c.Text) is { } command)
            await OnCommandAsync(c, command, cancellationToken);
        return Result.Success();
    }

    private async Task OnMembershipAsync(HandleTelegramUpdateCommand c, string status, CancellationToken cancellationToken)
    {
        if (status is "left" or "kicked")
            await ChatLinking.UnlinkAsync(db, c.ChatId, cancellationToken);
        else if (c.IsGroup && status is "member" or "administrator")
            await LinkAndAnswerAsync(c, Audience.Family, cancellationToken);
    }

    private async Task OnButtonAsync(HandleTelegramUpdateCommand c, string callbackId, string data, CancellationToken cancellationToken)
    {
        if (data.StartsWith(BotTexts.ChoosePrefix, StringComparison.Ordinal)
            && Enum.TryParse<Audience>(data[BotTexts.ChoosePrefix.Length..], out var person) && person != Audience.Family && !c.IsGroup)
        {
            await bot.TryAnswerButtonAsync(callbackId, "👌", cancellationToken);
            await LinkAndAnswerAsync(c, person, cancellationToken);
            return;
        }

        if (data.StartsWith(TodoTexts.DonePrefix, StringComparison.Ordinal) && Guid.TryParse(data[TodoTexts.DonePrefix.Length..], out var taskId))
        {
            var linked = await db.Chats.AsNoTracking().AnyAsync(x => x.ChatId == c.ChatId, cancellationToken);
            var answer = linked ? BotTexts.Done(await completion.CompleteAsync(taskId, cancellationToken)) : BotTexts.NotLinked;
            await bot.TryAnswerButtonAsync(callbackId, answer, cancellationToken);
            return;
        }

        await bot.TryAnswerButtonAsync(callbackId, "🤷", cancellationToken);
    }

    private async Task OnCommandAsync(HandleTelegramUpdateCommand c, string command, CancellationToken cancellationToken)
    {
        var audience = await db.Chats.AsNoTracking().Where(x => x.ChatId == c.ChatId).Select(x => (Audience?)x.Audience)
            .FirstOrDefaultAsync(cancellationToken);
        switch (command)
        {
            case "/start" when c.IsGroup:
                await LinkAndAnswerAsync(c, Audience.Family, cancellationToken);
                break;
            case "/start" when audience is null:
                await bot.TrySendAsync(c.ChatId, BotTexts.Ask, BotTexts.AskButtons, cancellationToken);
                break;
            case "/start":
                await Reply(c, BotTexts.AlreadyLinked(audience.Value) + "\n\n" + BotTexts.Help(Settings), cancellationToken);
                break;
            case "/today" when audience is not null:
                await SendTodayAsync(c.ChatId, audience.Value, cancellationToken);
                break;
            case "/help" when audience is not null:
                await Reply(c, BotTexts.Help(Settings), cancellationToken);
                break;
            default:
                if (audience is null && !c.IsGroup)
                    await Reply(c, BotTexts.NotLinked, cancellationToken);
                break;
        }
    }

    private async Task SendTodayAsync(long chatId, Audience audience, CancellationToken cancellationToken)
    {
        var active = await todos.ListActiveAsync(cancellationToken);
        var today = KyivTime.Today(clock);
        if (audience == Audience.Family)
        {
            var common = TodoTexts.Common(active, today, site.Page("todos")) ?? BotTexts.NothingToday;
            await bot.TrySendAsync(chatId, common, NoButtons, cancellationToken);
            return;
        }

        var morning = TodoTexts.Morning(
            DigestPlanner.Assignee(audience), TextFormat.Person(audience.ToString()), active, today);
        await bot.TrySendAsync(chatId, morning?.Text ?? BotTexts.NothingToday, morning?.Buttons ?? NoButtons, cancellationToken);
    }

    private async Task LinkAndAnswerAsync(HandleTelegramUpdateCommand c, Audience audience, CancellationToken cancellationToken)
    {
        var (linked, current) = await ChatLinking.LinkAsync(db, audience, c.ChatId, c.ChatTitle, clock.GetUtcNow(), cancellationToken);
        var answer = (linked, current) switch
        {
            (true, null) => audience == Audience.Family ? BotTexts.FamilyLinked : BotTexts.Linked(audience, Settings),
            (_, { } other) => BotTexts.AlreadyLinked(other),
            _ => BotTexts.Taken(audience, Settings),
        };
        await Reply(c, answer, cancellationToken);
    }

    private Task Reply(HandleTelegramUpdateCommand c, string text, CancellationToken cancellationToken) =>
        bot.TrySendAsync(c.ChatId, text, NoButtons, cancellationToken);

    private string Settings => site.Page("notifications");

    /// <summary><c>/today@home_bot extra</c> → <c>/today</c>; null for plain text.</summary>
    private static string? Command(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || text[0] != '/')
            return null;
        var word = text.Split([' ', '\n'], 2)[0];
        var at = word.IndexOf('@', StringComparison.Ordinal);
        return (at < 0 ? word : word[..at]).ToLowerInvariant();
    }
}

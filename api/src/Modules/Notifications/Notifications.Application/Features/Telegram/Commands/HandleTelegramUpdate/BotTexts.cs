using Notifications.Application.Digests.Texts;
using Notifications.Application.Sending;
using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;

/// <summary>What the bot answers to commands and buttons (Russian).</summary>
public static class BotTexts
{
    public const string ChoosePrefix = "me:";

    public const string Ask = "👋 Привет! Я бот нашего дома: утром пришлю дела, вечером — покупки, по воскресеньям и в конце месяца — расходы.\n\nКто вы?";

    public const string NotLinked = "Сначала напишите /start, чтобы подключить этот чат.";

    public const string NothingToday = "На сегодня дел нет 🎉";

    public const string FamilyLinked =
        "✅ Группа подключена как семейная: сюда будут приходить общие дела, покупки и сводки по расходам.";

    public static IReadOnlyList<TelegramButton> AskButtons { get; } =
    [
        new("Я Виталий", ChoosePrefix + nameof(Audience.Vitaliy)),
        new("Я Маша", ChoosePrefix + nameof(Audience.Maria)),
    ];

    public static string Help(string settingsUrl) =>
        "Команды:\n/today — дела на сегодня\n/help — эта подсказка\n\n"
        + $"Расписание и отвязка чатов — на сайте: {TextFormat.Link(settingsUrl, "Телеграм")}";

    public static string Linked(Audience audience, string settingsUrl) =>
        $"✅ Готово, {TextFormat.Person(audience.ToString())}! Чат подключён.\n\n{Help(settingsUrl)}";

    public static string AlreadyLinked(Audience audience) =>
        $"Этот чат уже подключён: {Who(audience)}.";

    public static string Taken(Audience audience, string settingsUrl) =>
        $"{Who(audience)} — уже подключён другой чат. Отвязать его можно на сайте: {TextFormat.Link(settingsUrl, "Телеграм")}";

    public static string Done(Todos.Contracts.TodoCompletionOutcome outcome) => outcome switch
    {
        Todos.Contracts.TodoCompletionOutcome.Completed => "✅ Готово!",
        Todos.Contracts.TodoCompletionOutcome.AlreadyClosed => "Это дело уже закрыто.",
        _ => "Этого дела уже нет.",
    };

    private static string Who(Audience audience) => audience == Audience.Family ? "семейная группа" : TextFormat.Person(audience.ToString());
}

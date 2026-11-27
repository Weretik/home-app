using Notifications.Application.Digests.Texts;
using Notifications.Application.Sending;
using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;

/// <summary>What the bot answers to commands and buttons (Ukrainian).</summary>
public static class BotTexts
{
    public const string ChoosePrefix = "me:";

    public const string Ask = "👋 Привіт! Я бот нашого дому: зранку надішлю справи, увечері — покупки, щонеділі й наприкінці місяця — витрати.\n\nХто ви?";

    public const string NotLinked = "Спершу напишіть /start, щоб підключити цей чат.";

    public const string NothingToday = "На сьогодні справ немає 🎉";

    public const string FamilyLinked =
        "✅ Групу підключено як сімейну: сюди надходитимуть спільні справи, покупки й підсумки витрат.";

    public static IReadOnlyList<TelegramButton> AskButtons { get; } =
    [
        new("Я Віталій", ChoosePrefix + nameof(Audience.Vitaliy)),
        new("Я Маша", ChoosePrefix + nameof(Audience.Maria)),
    ];

    public static string Help(string settingsUrl) =>
        "Команди:\n/today — справи на сьогодні\n/help — ця підказка\n\n"
        + $"Розклад і відв’язування чатів — на сайті: {TextFormat.Link(settingsUrl, "Телеграм")}";

    public static string Linked(Audience audience, string settingsUrl) =>
        $"✅ Готово, {TextFormat.Address(audience.ToString())}! Чат підключено.\n\n{Help(settingsUrl)}";

    public static string AlreadyLinked(Audience audience) =>
        $"Цей чат уже підключено: {Who(audience)}.";

    public static string Taken(Audience audience, string settingsUrl) =>
        $"{Who(audience)} — уже підключено інший чат. Відв’язати його можна на сайті: {TextFormat.Link(settingsUrl, "Телеграм")}";

    public static string Done(Todos.Contracts.TodoCompletionOutcome outcome) => outcome switch
    {
        Todos.Contracts.TodoCompletionOutcome.Completed => "✅ Готово!",
        Todos.Contracts.TodoCompletionOutcome.AlreadyClosed => "Цю справу вже закрито.",
        _ => "Цієї справи вже немає.",
    };

    private static string Who(Audience audience) => audience == Audience.Family ? "сімейна група" : TextFormat.Person(audience.ToString());
}

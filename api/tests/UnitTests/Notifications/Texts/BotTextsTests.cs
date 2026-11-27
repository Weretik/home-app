using Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;
using Notifications.Domain.Chats;
using Todos.Contracts;

namespace UnitTests.Notifications.Texts;

public sealed class BotTextsTests
{
    [Fact]
    public void The_Who_Are_You_Buttons_AreUkrainian_AndKeepTheirCallbackData()
    {
        Assert.EndsWith("Хто ви?", BotTexts.Ask);
        Assert.Equal(["Я Віталій", "Я Маша"], BotTexts.AskButtons.Select(b => b.Text));
        Assert.Equal(["me:Vitaliy", "me:Maria"], BotTexts.AskButtons.Select(b => b.Data));
    }

    [Fact]
    public void The_Answers_AddressThePerson_AndNameTheFamilyGroup()
    {
        Assert.StartsWith("✅ Готово, Віталію! Чат підключено.", BotTexts.Linked(Audience.Vitaliy, "https://web.example/telegram"));
        Assert.Equal("Цей чат уже підключено: сімейна група.", BotTexts.AlreadyLinked(Audience.Family));
        Assert.Equal("Цю справу вже закрито.", BotTexts.Done(TodoCompletionOutcome.AlreadyClosed));
    }
}

using Notifications.Application.Digests.Texts;
using Todos.Contracts;

namespace UnitTests.Notifications.Texts;

public sealed class TodoTextsTests
{
    private static readonly DateOnly Today = new(2026, 12, 2);
    private static readonly Guid Overdue = Guid.Parse("0193b6a0-0000-7000-8000-000000000001");

    private static readonly OpenTodo[] Active =
    [
        new(Overdue, "Оплатить <свет>", TodoAssignee.Vitaliy, new DateOnly(2026, 11, 30), true),
        new(Guid.NewGuid(), "Забрать посылку", TodoAssignee.Vitaliy, Today, false),
        new(Guid.NewGuid(), "Через неделю", TodoAssignee.Vitaliy, Today.AddDays(7), false),
        new(Guid.NewGuid(), "Без срока", TodoAssignee.Maria, null, false),
        new(Guid.NewGuid(), "Вынести мусор", TodoAssignee.Shared, Today, false),
    ];

    [Fact]
    public void The_Morning_ShowsOverdueTodayAndShared_WithADoneButtonEach()
    {
        var (text, buttons) = TodoTexts.Morning(TodoAssignee.Vitaliy, "Виталий", Active, Today)!.Value;

        Assert.Equal(
            "☀️ <b>Доброе утро, Виталий!</b> Дела на сегодня:\n\n"
            + "🔴 <b>Просрочено</b>\n• ❗ Оплатить &lt;свет&gt; — с 30.11\n\n"
            + "📌 <b>Сегодня</b>\n• Забрать посылку\n\n"
            + "👥 <b>Общие</b>\n• Вынести мусор",
            text);
        Assert.Equal(3, buttons.Count);
        Assert.Equal("done:" + Overdue.ToString("N"), buttons[0].Data);
        Assert.StartsWith("✅ Оплатить", buttons[0].Text);
    }

    [Fact]
    public void A_Morning_WithNothingForToday_IsNotSent()
    {
        Assert.Null(TodoTexts.Morning(TodoAssignee.Maria, "Маша", [Active[3]], Today));
    }

    [Fact]
    public void The_Common_Message_CountsPerPerson_AndListsTheOverdue()
    {
        var text = TodoTexts.Common(Active, Today, "https://web.example/todos")!;

        Assert.Contains("Открыто: Виталий — 3, Маша — 1, Общее — 1", text);
        Assert.Contains("• Оплатить &lt;свет&gt; — Виталий, с 30.11", text);
        Assert.EndsWith("<a href=\"https://web.example/todos\">Открыть дела</a>", text);
        Assert.Contains("Просроченных нет", TodoTexts.Common([Active[1]], Today, "u")!);
        Assert.Null(TodoTexts.Common([], Today, "u"));
    }
}

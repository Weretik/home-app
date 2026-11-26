using System.Text;
using Notifications.Application.Sending;
using Todos.Contracts;

namespace Notifications.Application.Digests.Texts;

/// <summary>
/// The to-do messages (spec notifications/001, R-008 and R-009). Pure: the same tasks and day give the same text.
/// </summary>
public static class TodoTexts
{
    public const int MaxButtons = 8;
    public const string DonePrefix = "done:";
    private const int ButtonTitleLength = 40;

    /// <summary>
    /// A person's morning: their overdue and today's tasks, then the shared ones; a «✅» button per task. Null when
    /// there is nothing for today.
    /// </summary>
    public static (string Text, IReadOnlyList<TelegramButton> Buttons)? Morning(
        TodoAssignee person, string name, IReadOnlyList<OpenTodo> active, DateOnly today)
    {
        var mine = active.Where(t => t.Assignee == person && t.DueDate <= today).ToList();
        var shared = active.Where(t => t.Assignee == TodoAssignee.Shared && t.DueDate <= today).ToList();
        if (mine.Count == 0 && shared.Count == 0)
            return null;

        var text = new StringBuilder($"☀️ <b>Доброе утро, {TextFormat.Escape(name)}!</b> Дела на сегодня:\n");
        AppendSection(text, "🔴 <b>Просрочено</b>", mine.Where(t => t.DueDate < today).ToList(), today);
        AppendSection(text, "📌 <b>Сегодня</b>", mine.Where(t => t.DueDate == today).ToList(), today);
        AppendSection(text, "👥 <b>Общие</b>", shared, today);

        var buttons = mine.Concat(shared)
            .Take(MaxButtons)
            .Select(t => new TelegramButton("✅ " + TextFormat.Short(t.Title, ButtonTitleLength), DonePrefix + t.Id.ToString("N")))
            .ToList();
        return (text.ToString().TrimEnd(), buttons);
    }

    /// <summary>The common message: active tasks per assignee and everything overdue. Null when nothing is active.</summary>
    public static string? Common(IReadOnlyList<OpenTodo> active, DateOnly today, string todosUrl)
    {
        if (active.Count == 0)
            return null;

        var text = new StringBuilder("📋 <b>Дела семьи</b>\n");
        var counts = new[] { TodoAssignee.Vitaliy, TodoAssignee.Maria, TodoAssignee.Shared }
            .Select(a => $"{TextFormat.Person(a.ToString())} — {active.Count(t => t.Assignee == a)}");
        text.Append("Открыто: ").Append(string.Join(", ", counts)).Append('\n');

        var overdue = active.Where(t => t.DueDate < today).ToList();
        if (overdue.Count == 0)
            text.Append("\nПросроченных нет 👍\n");
        else
        {
            text.Append("\n🔴 <b>Просрочено</b>\n");
            TextFormat.AppendLines(text, [.. overdue.Select(t =>
                $"• {TextFormat.Escape(t.Title)} — {TextFormat.Person(t.Assignee.ToString())}, с {TextFormat.Day(t.DueDate!.Value)}")]);
        }

        text.Append('\n').Append(TextFormat.Link(todosUrl, "Открыть дела"));
        return text.ToString();
    }

    private static void AppendSection(StringBuilder text, string heading, IReadOnlyList<OpenTodo> tasks, DateOnly today)
    {
        if (tasks.Count == 0)
            return;
        text.Append('\n').Append(heading).Append('\n');
        TextFormat.AppendLines(text, [.. tasks.Select(t => Line(t, today))]);
    }

    private static string Line(OpenTodo task, DateOnly today)
    {
        var line = $"• {(task.Urgent ? "❗ " : string.Empty)}{TextFormat.Escape(task.Title)}";
        return task.DueDate < today ? $"{line} — с {TextFormat.Day(task.DueDate.Value)}" : line;
    }
}

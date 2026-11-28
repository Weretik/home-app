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

        var text = new StringBuilder($"☀️ <b>Доброго ранку, {TextFormat.Escape(name)}!</b> Справи на сьогодні:\n");
        AppendSection(text, "🔴 <b>Прострочено</b>", mine.Where(t => t.DueDate < today).ToList(), today);
        AppendSection(text, "📌 <b>Сьогодні</b>", mine.Where(t => t.DueDate == today).ToList(), today);
        AppendSection(text, "👥 <b>Спільні</b>", shared, today);

        var buttons = mine.Concat(shared)
            .Take(MaxButtons)
            .Select(t => new TelegramButton("✅ " + TextFormat.Short(t.Title, ButtonTitleLength), DonePrefix + t.Id.ToString("N")))
            .ToList();
        return (text.ToString().TrimEnd(), buttons);
    }

    /// <summary>
    /// The common message: every active task, in full, grouped by assignee (overdue first, then by date). In a
    /// person's own chat only their tasks and the shared ones, never the other person's (R-009). Null when there is
    /// nothing to list.
    /// </summary>
    public static string? Common(IReadOnlyList<OpenTodo> active, DateOnly today, string todosUrl, TodoAssignee? person = null)
    {
        (TodoAssignee Assignee, string Heading)[] groups = person is { } me
            ? [(me, "👤 <b>Мої</b>"), (TodoAssignee.Shared, "👥 <b>Спільні</b>")]
            :
            [
                (TodoAssignee.Vitaliy, "👤 <b>Віталій</b>"),
                (TodoAssignee.Maria, "👤 <b>Маша</b>"),
                (TodoAssignee.Shared, "👥 <b>Спільні</b>"),
            ];
        var listed = groups
            .Select(g => (g.Heading, Tasks: active
                .Where(t => t.Assignee == g.Assignee)
                .OrderBy(t => t.DueDate ?? DateOnly.MaxValue)
                .ThenByDescending(t => t.Urgent)
                .ToList()))
            .Where(g => g.Tasks.Count > 0)
            .ToList();
        if (listed.Count == 0)
            return null;

        var text = new StringBuilder(person is null ? "📋 <b>Справи родини</b>\n" : "📋 <b>Усі відкриті справи</b>\n");
        foreach (var (heading, tasks) in listed)
        {
            text.Append('\n').Append(heading).Append(" (").Append(tasks.Count).Append(")\n");
            TextFormat.AppendLines(text, [.. tasks.Select(t => CommonLine(t, today))]);
        }

        text.Append('\n').Append(TextFormat.Link(todosUrl, "Відкрити справи"));
        return text.ToString();
    }

    private static string CommonLine(OpenTodo task, DateOnly today)
    {
        var title = $"{(task.Urgent ? "❗ " : string.Empty)}{TextFormat.Escape(task.Title)}";
        return task.DueDate switch
        {
            { } due when due < today => $"• 🔴 {title} — з {TextFormat.Day(due)}",
            { } due when due == today => $"• {title} — сьогодні",
            { } due => $"• {title} — до {TextFormat.Day(due)}",
            null => $"• {title}",
        };
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
        return task.DueDate < today ? $"{line} — з {TextFormat.Day(task.DueDate.Value)}" : line;
    }
}

using Todos.Application.Features.Summary.Queries.GetTodoSummary;
using Todos.Application.Features.Tasks.Reading;
using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace UnitTests.Todos.Summary;

public sealed class SummaryFocusTests
{
    private static readonly DateOnly Today = new(2026, 11, 26);
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    private static TodoTaskItem Task(
        string title, TodoPriority priority = TodoPriority.Normal, DateOnly? due = null, TodoStatus status = TodoStatus.Todo, int createdDay = 1) =>
        new(Guid.NewGuid(), title, null, Assignee.Shared, null, status, priority, due, null, null, null,
            Created.AddDays(createdDay), Created, null, [], 0, 0, 1);

    [Fact]
    public void Overdue_ComeFirst_ByDueDate_ThenToday_ThenUrgent()
    {
        var tasks = new[]
        {
            Task("urgent later", TodoPriority.Urgent, Today.AddDays(3)),
            Task("today low", TodoPriority.Low, Today),
            Task("overdue recent", TodoPriority.Urgent, Today.AddDays(-1)),
            Task("today urgent", TodoPriority.Urgent, Today),
            Task("urgent no date", TodoPriority.Urgent),
            Task("overdue old", TodoPriority.Low, Today.AddDays(-10)),
            Task("normal later", TodoPriority.Normal, Today.AddDays(1)),
        };

        var focus = SummaryFocus.Pick(tasks, Today).Select(t => t.Title);

        Assert.Equal(["overdue old", "overdue recent", "today urgent", "today low", "urgent later", "urgent no date"], focus);
    }

    [Fact]
    public void Archived_Tasks_AreNeverInFocus()
    {
        var tasks = new[] { Task("done overdue", due: Today.AddDays(-2), status: TodoStatus.Done) };

        Assert.Empty(SummaryFocus.Pick(tasks, Today));
    }

    [Fact]
    public void At_Most_EightTasks_AreShown()
    {
        var tasks = Enumerable.Range(1, 12).Select(i => Task($"t{i}", due: Today.AddDays(-i))).ToList();

        var focus = SummaryFocus.Pick(tasks, Today);

        Assert.Equal(8, focus.Count);
        Assert.Equal("t12", focus[0].Title);
    }
}

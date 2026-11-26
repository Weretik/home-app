using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace UnitTests.Todos.Domain;

public sealed class TodoTaskTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    internal static TodoTask NewTask(
        string title = "Приклеить деревяшки",
        string? notes = null,
        TodoStatus status = TodoStatus.Todo,
        DateOnly? dueDate = null,
        IReadOnlyCollection<string>? steps = null) =>
        TodoTask.Create(title, notes, Assignee.Maria, Person.Vitaliy, status, TodoPriority.High, dueDate, null, steps ?? [], Now).Value;

    [Fact]
    public void A_New_Task_IsTrimmed_Active_AndHasNoCompletionTime()
    {
        var task = NewTask(title: "  Купить лампочки  ", notes: "   ");

        Assert.Equal("Купить лампочки", task.Title);
        Assert.Null(task.Notes);
        Assert.Equal(TodoStatus.Todo, task.Status);
        Assert.Null(task.CompletedAtUtc);
        Assert.Equal(Now, task.CreatedAtUtc);
        Assert.Equal(Now, task.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_Empty_Title_IsRefused(string title)
    {
        var result = TodoTask.Create(title, null, Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, null, null, [], Now);

        Assert.Equal("todos.title_invalid", result.Error!.Code);
    }

    [Fact]
    public void A_Title_Of200Characters_IsAccepted_And201_IsRefused()
    {
        Assert.True(TodoTask.Create(new string('a', 200), null, Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, null, null, [], Now).IsSuccess);
        Assert.Equal("todos.title_invalid",
            TodoTask.Create(new string('a', 201), null, Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, null, null, [], Now).Error!.Code);
    }

    [Fact]
    public void Notes_LongerThan4000Characters_AreRefused()
    {
        var result = TodoTask.Create("x", new string('n', 4001), Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, null, null, [], Now);

        Assert.Equal("todos.notes_too_long", result.Error!.Code);
    }

    [Theory]
    [InlineData("1999-12-31")]
    [InlineData("2101-01-01")]
    public void A_DueDate_OutsideTheRange_IsRefused(string date)
    {
        var result = TodoTask.Create("x", null, Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, DateOnly.Parse(date), null, [], Now);

        Assert.Equal("todos.due_date_out_of_range", result.Error!.Code);
    }

    [Fact]
    public void A_Task_CreatedDone_IsArchived_WithTheCompletionTime()
    {
        var task = NewTask(status: TodoStatus.Done);

        Assert.True(task.IsArchived);
        Assert.Equal(Now, task.CompletedAtUtc);
    }

    [Fact]
    public void Done_ThenReopened_ClearsTheCompletionTime()
    {
        var task = NewTask();

        task.ChangeStatus(TodoStatus.Done, Now.AddHours(1));
        Assert.Equal(Now.AddHours(1), task.CompletedAtUtc);

        task.ChangeStatus(TodoStatus.Todo, Now.AddHours(2));
        Assert.Null(task.CompletedAtUtc);
        Assert.False(task.IsArchived);
        Assert.Equal(Now.AddHours(2), task.UpdatedAtUtc);
    }

    [Fact]
    public void Moving_BetweenArchivedStatuses_KeepsTheFirstCompletionTime()
    {
        var task = NewTask();
        task.ChangeStatus(TodoStatus.Done, Now.AddHours(1));

        task.ChangeStatus(TodoStatus.Cancelled, Now.AddHours(5));

        Assert.Equal(TodoStatus.Cancelled, task.Status);
        Assert.Equal(Now.AddHours(1), task.CompletedAtUtc);
    }

    [Fact]
    public void Moving_BetweenActiveStatuses_SetsNoCompletionTime()
    {
        var task = NewTask();

        task.ChangeStatus(TodoStatus.InProgress, Now.AddHours(1));
        task.ChangeStatus(TodoStatus.Paused, Now.AddHours(2));

        Assert.Null(task.CompletedAtUtc);
    }

    [Fact]
    public void Update_ReplacesTheFields_AndAppliesTheStatusRules()
    {
        var task = NewTask();
        var category = Guid.NewGuid();

        var result = task.Update(" Новое ", " заметка ", Assignee.Shared, null, TodoStatus.Done, TodoPriority.Low,
            new DateOnly(2026, 12, 1), category, Now.AddHours(3));

        Assert.True(result.IsSuccess);
        Assert.Equal("Новое", task.Title);
        Assert.Equal("заметка", task.Notes);
        Assert.Equal(Assignee.Shared, task.Assignee);
        Assert.Null(task.Author);
        Assert.Equal(TodoPriority.Low, task.Priority);
        Assert.Equal(category, task.CategoryId);
        Assert.Equal(Now.AddHours(3), task.CompletedAtUtc);
        Assert.Equal(Now.AddHours(3), task.UpdatedAtUtc);
    }

    [Fact]
    public void A_Refused_Update_ChangesNothing()
    {
        var task = NewTask();

        var result = task.Update("", null, Assignee.Shared, null, TodoStatus.Done, TodoPriority.Low, null, null, Now.AddHours(1));

        Assert.Equal("todos.title_invalid", result.Error!.Code);
        Assert.Equal("Приклеить деревяшки", task.Title);
        Assert.Equal(TodoStatus.Todo, task.Status);
    }
}

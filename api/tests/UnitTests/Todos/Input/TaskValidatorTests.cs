using Todos.Application.Features.Tasks.Commands.CreateTodoTask;
using Todos.Application.Features.Tasks.Commands.UpdateTodoTask;
using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace UnitTests.Todos.Input;

public sealed class TaskValidatorTests
{
    private static readonly CreateTodoTaskValidator Create = new();
    private static readonly UpdateTodoTaskValidator Update = new();

    private static CreateTodoTaskCommand NewTask(string key = "k", string title = "Задача", IReadOnlyList<string>? steps = null) =>
        new(key, title, null, Assignee.Maria, null, null, TodoPriority.Normal, null, null, steps ?? []);

    [Fact]
    public void A_Plain_Task_IsValid()
    {
        Assert.True(Create.Validate(NewTask()).IsValid);
    }

    [Fact]
    public void A_Create_WithoutIdempotencyKey_IsInvalid()
    {
        Assert.Contains(Create.Validate(NewTask(key: "")).Errors, e => e.PropertyName == "IdempotencyKey");
    }

    [Fact]
    public void A_Create_WithTooManyOrEmptySteps_IsInvalid()
    {
        Assert.False(Create.Validate(NewTask(steps: Enumerable.Range(0, 51).Select(i => $"s{i}").ToList())).IsValid);
        Assert.False(Create.Validate(NewTask(steps: ["ok", ""])).IsValid);
    }

    [Fact]
    public void An_Unknown_Enum_Value_IsInvalid()
    {
        var command = NewTask() with { Assignee = (Assignee)7, Status = (TodoStatus)9 };

        var errors = Create.Validate(command).Errors.Select(e => e.PropertyName).ToList();

        Assert.Contains("Assignee", errors);
        Assert.Contains("Status", errors);
    }

    [Fact]
    public void An_Update_WithoutVersion_IsInvalid()
    {
        var command = new UpdateTodoTaskCommand(Guid.NewGuid(), 0, "x", null, Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, null, null);

        Assert.Contains(Update.Validate(command).Errors, e => e.PropertyName == "Version");
    }
}

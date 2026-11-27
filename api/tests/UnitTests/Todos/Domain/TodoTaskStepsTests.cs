using Todos.Domain.Parties;
using Todos.Domain.Tasks;
using Todos.Domain.Tasks.Identifiers;

namespace UnitTests.Todos.Domain;

public sealed class TodoTaskStepsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Steps_GivenAtCreation_KeepTheirOrder()
    {
        var task = TodoTaskTests.NewTask(steps: ["Купить клей", " Приклеить ", "Покрасить"]);

        Assert.Equal(["Купить клей", "Приклеить", "Покрасить"], task.Steps.Select(s => s.Title));
        Assert.All(task.Steps, s => Assert.False(s.IsDone));
    }

    [Fact]
    public void More_Than50_Steps_AreRefused_AtCreation()
    {
        var steps = Enumerable.Range(1, 51).Select(i => $"шаг {i}").ToList();

        var result = TodoTask.Create("x", null, Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, null, null, steps, Now);

        Assert.Equal("todos.too_many_steps", result.Error!.Code);
    }

    [Fact]
    public void An_Empty_Step_IsRefused_AtCreation()
    {
        var result = TodoTask.Create("x", null, Assignee.Shared, null, TodoStatus.Todo, TodoPriority.Normal, null, null, ["ok", " "], Now);

        Assert.Equal("todos.title_invalid", result.Error!.Code);
    }

    [Fact]
    public void A_Step_IsAdded_AtTheEnd_AndTouchesTheTask()
    {
        var task = TodoTaskTests.NewTask(steps: ["первый"]);

        var added = task.AddStep(" второй ", Now.AddMinutes(5));

        Assert.True(added.IsSuccess);
        Assert.Equal(["первый", "второй"], task.Steps.Select(s => s.Title));
        Assert.Equal(Now.AddMinutes(5), task.UpdatedAtUtc);
    }

    [Fact]
    public void The_51st_Step_IsRefused()
    {
        var task = TodoTaskTests.NewTask(steps: Enumerable.Range(1, 50).Select(i => $"шаг {i}").ToList());

        Assert.Equal("todos.too_many_steps", task.AddStep("ещё", Now).Error!.Code);
        Assert.Equal(50, task.Steps.Count);
    }

    [Fact]
    public void A_Step_IsTicked_Renamed_AndRemoved()
    {
        var task = TodoTaskTests.NewTask(steps: ["a", "b", "c"]);
        var b = task.Steps[1].Id;

        Assert.True(task.UpdateStep(b, "B", true, Now.AddMinutes(1)).IsSuccess);
        Assert.Equal("B", task.Steps[1].Title);
        Assert.True(task.Steps[1].IsDone);

        Assert.True(task.RemoveStep(task.Steps[0].Id, Now.AddMinutes(2)).IsSuccess);
        Assert.Equal(["B", "c"], task.Steps.Select(s => s.Title));
        Assert.Equal(Now.AddMinutes(2), task.UpdatedAtUtc);
    }

    [Fact]
    public void An_Unknown_Step_IsNotFound()
    {
        var task = TodoTaskTests.NewTask();

        Assert.Equal("todos.step_not_found", task.UpdateStep(new TaskStepId(Guid.NewGuid()), "x", true, Now).Error!.Code);
        Assert.Equal("todos.step_not_found", task.RemoveStep(new TaskStepId(Guid.NewGuid()), Now).Error!.Code);
    }

    [Fact]
    public void A_Step_Renamed_ToNothing_IsRefused()
    {
        var task = TodoTaskTests.NewTask(steps: ["a"]);

        Assert.Equal("todos.title_invalid", task.UpdateStep(task.Steps[0].Id, " ", false, Now).Error!.Code);
        Assert.Equal("a", task.Steps[0].Title);
    }
}

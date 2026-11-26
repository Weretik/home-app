using Todos.Application.Features.Tasks.Reading;

namespace Todos.Application.Features.Summary.Queries.GetTodoSummary;

/// <summary>
/// The home page block: today's date in Kyiv, the counts of Маша, Виталий and Общее (always all three, in that
/// order), how many tasks were done in the last seven days, and up to eight tasks to look at first.
/// </summary>
public sealed record TodoSummary(
    DateOnly Today,
    IReadOnlyList<AssigneeCounts> Assignees,
    int DoneLast7Days,
    IReadOnlyList<TodoTaskItem> Focus);

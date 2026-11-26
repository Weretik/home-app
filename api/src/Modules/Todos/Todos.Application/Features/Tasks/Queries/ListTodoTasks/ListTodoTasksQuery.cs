using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Domain.Parties;

namespace Todos.Application.Features.Tasks.Queries.ListTodoTasks;

/// <summary>
/// The active tasks or the archive (<see cref="View"/> <c>active</c> or <c>archive</c>), optionally only of some
/// assignees, of one category or of none, with every word of <see cref="Search"/> in the title, and due between
/// <see cref="DueFrom"/> and <see cref="DueTo"/> (both included, either may be open) or without a due date. The archive
/// can also be narrowed by the Kyiv day of completion, <see cref="CompletedFrom"/> to <see cref="CompletedTo"/>.
/// </summary>
public sealed record ListTodoTasksQuery(
    string View,
    IReadOnlyList<Assignee> Assignees,
    Guid? CategoryId,
    bool WithoutCategory,
    string? Search,
    DateOnly? DueFrom,
    DateOnly? DueTo,
    bool WithoutDueDate,
    DateOnly? CompletedFrom,
    DateOnly? CompletedTo,
    int Page,
    int PageSize) : IQuery<Result<TodoTaskList>>;

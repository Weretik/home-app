using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Domain.Parties;

namespace Todos.Application.Features.Tasks.Queries.ListTodoTasks;

/// <summary>
/// The active tasks or the archive (<see cref="View"/> <c>active</c> or <c>archive</c>), optionally only of some
/// assignees, of one category or of none, and with every word of <see cref="Search"/> in the title.
/// </summary>
public sealed record ListTodoTasksQuery(
    string View,
    IReadOnlyList<Assignee> Assignees,
    Guid? CategoryId,
    bool WithoutCategory,
    string? Search,
    int Page,
    int PageSize) : IQuery<Result<TodoTaskList>>;

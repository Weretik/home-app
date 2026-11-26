using Todos.Application.Features.Tasks.Reading;

namespace Todos.Application.Features.Tasks.Queries.ListTodoTasks;

/// <summary>One page of active or archived tasks and how many match the filter in all.</summary>
public sealed record TodoTaskList(IReadOnlyList<TodoTaskItem> Items, int Page, int PageSize, int TotalCount);

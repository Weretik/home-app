namespace Todos.Contracts;

/// <summary>An active task, for the morning messages of the Telegram bot.</summary>
public sealed record OpenTodo(Guid Id, string Title, TodoAssignee Assignee, DateOnly? DueDate, bool Urgent);

using Todos.Domain.Parties;

namespace Todos.Application.Features.Summary.Queries.GetTodoSummary;

/// <summary>How many active tasks an assignee has, how many of them are overdue and how many are due today.</summary>
public sealed record AssigneeCounts(Assignee Assignee, int Active, int Overdue, int DueToday);

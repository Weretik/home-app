using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Planned.Queries.ListPlannedExpenses;

/// <summary>The planned expenses of one calendar month, by name, with their total.</summary>
public sealed record ListPlannedExpensesQuery(int Year, int Month) : IQuery<Result<PlannedExpenseList>>;

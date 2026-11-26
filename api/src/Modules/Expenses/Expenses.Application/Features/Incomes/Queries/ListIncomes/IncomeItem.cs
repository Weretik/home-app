using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Incomes.Queries.ListIncomes;

/// <summary>One income row of the month list.</summary>
public sealed record IncomeItem(Guid Id, uint Version, DateOnly Date, Person Person, decimal Amount, string? Comment);

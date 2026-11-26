using Expenses.Domain.Parties;

namespace Expenses.Api.Endpoints.Incomes;

/// <summary>The fields of an income, for adding or changing one.</summary>
internal sealed record IncomeRequest(DateOnly Date, Person Person, decimal Amount, string? Comment);

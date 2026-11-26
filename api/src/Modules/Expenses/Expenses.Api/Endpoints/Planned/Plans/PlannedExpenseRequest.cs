using Expenses.Domain.Parties;

namespace Expenses.Api.Endpoints.Planned.Plans;

/// <summary>The fields of a planned expense, for adding or changing one: its month, name, category, payer and amount.</summary>
internal sealed record PlannedExpenseRequest(int Year, int Month, string Name, Guid CategoryId, Payer Payer, decimal Amount);

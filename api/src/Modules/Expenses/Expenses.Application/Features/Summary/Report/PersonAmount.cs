using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Summary.Report;

/// <summary>How much one person earned in the month.</summary>
public sealed record PersonAmount(Person Person, decimal Amount);

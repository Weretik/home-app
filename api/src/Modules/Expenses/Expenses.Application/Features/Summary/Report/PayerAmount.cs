using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Summary.Report;

/// <summary>How much one payer (Виталий, Мария or the shared budget) spent in the month.</summary>
public sealed record PayerAmount(Payer Payer, decimal Amount);

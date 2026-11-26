namespace Expenses.Application.Features.Summary.Report;

/// <summary>How much went to one category in the month.</summary>
public sealed record CategoryAmount(Guid CategoryId, string Name, decimal Amount);

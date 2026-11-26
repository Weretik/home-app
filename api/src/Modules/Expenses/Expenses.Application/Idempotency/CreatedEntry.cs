namespace Expenses.Application.Idempotency;

/// <summary>The id of a created expense, income or savings movement.</summary>
public sealed record CreatedEntry(Guid Id);

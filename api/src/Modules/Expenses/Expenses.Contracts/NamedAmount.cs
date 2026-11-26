namespace Expenses.Contracts;

/// <summary>An amount with what it belongs to: a category name, or a payer code (<c>Vitaliy</c>, <c>Maria</c>, <c>Shared</c>).</summary>
public sealed record NamedAmount(string Name, decimal Amount);

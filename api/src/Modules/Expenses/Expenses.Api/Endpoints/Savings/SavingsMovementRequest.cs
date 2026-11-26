using Expenses.Domain.Parties;

namespace Expenses.Api.Endpoints.Savings;

/// <summary>A savings movement: a positive amount puts money into the jar, a negative one takes it out.</summary>
internal sealed record SavingsMovementRequest(DateOnly Date, Person Person, decimal Amount, string? Comment);

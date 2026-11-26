using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Parties;

namespace Expenses.Domain.Entries;

/// <summary>Money received by one of the two people: one row of the old «Доходы» sheet. Incomes have no category.</summary>
public sealed class Income : Entity<Guid>, IAggregateRoot
{
    private Income()
    {
    }

    public DateOnly Date { get; private set; }

    public Person Person { get; private set; }

    public decimal Amount { get; private set; }

    public string? Comment { get; private set; }

    /// <summary>Set when the income came from an import; a unique index makes a repeated import skip it.</summary>
    public string? ImportKey { get; private init; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    public static Result<Income> Create(
        DateOnly date, Person person, decimal amount, string? comment, DateTimeOffset now, string? importKey = null)
    {
        var check = EntryRules.Check(date, amount, comment, importKey);
        if (check.IsFailure)
            return check.Error!;

        return new Income
        {
            Id = Guid.CreateVersion7(now),
            Date = date,
            Person = person,
            Amount = amount,
            Comment = EntryRules.Normalize(comment),
            ImportKey = importKey,
            CreatedAtUtc = now,
        };
    }
}

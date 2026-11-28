using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace Expenses.Domain.Savings;

/// <summary>
/// Money put into the savings jar (a positive amount) or taken out of it (a negative amount): one row of the old
/// «Копилка» sheet. The balance of the jar is the sum of all movements; it is never stored.
/// </summary>
public sealed class SavingsMovement : Entity<SavingsMovementId>, IAggregateRoot
{
    private SavingsMovement()
    {
    }

    public DateOnly Date { get; private set; }

    public Person Person { get; private set; }

    /// <summary>Plus: put in. Minus: taken out. Never zero.</summary>
    public decimal Amount { get; private set; }

    public string? Comment { get; private set; }

    public string? ImportKey { get; private init; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public SavingsDirection Direction => Amount > 0 ? SavingsDirection.Deposit : SavingsDirection.Withdrawal;

    public static Result<SavingsMovement> Create(
        DateOnly date, Person person, decimal amount, string? comment, DateTimeOffset now, string? importKey = null)
    {
        var check = EntryRules.CheckSigned(date, amount, comment, importKey);
        if (check.IsFailure)
            return check.Error!;

        return new SavingsMovement
        {
            Id = SavingsMovementId.New(now),
            Date = date,
            Person = person,
            Amount = amount,
            Comment = EntryRules.Normalize(comment),
            ImportKey = importKey,
            CreatedAtUtc = now,
        };
    }

    public Result Update(DateOnly date, Person person, decimal amount, string? comment)
    {
        var check = EntryRules.CheckSigned(date, amount, comment, ImportKey);
        if (check.IsFailure)
            return check;

        Date = date;
        Person = person;
        Amount = amount;
        Comment = EntryRules.Normalize(comment);
        return Result.Success();
    }
}

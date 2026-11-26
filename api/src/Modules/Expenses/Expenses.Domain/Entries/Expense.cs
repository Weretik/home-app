using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Parties;

namespace Expenses.Domain.Entries;

/// <summary>
/// Money spent: one row of the old «Расходы» sheet. It belongs to a category of the same module and is booked to a
/// person or to the household (<see cref="Payer"/>).
/// </summary>
public sealed class Expense : Entity<Guid>, IAggregateRoot
{
    private Expense()
    {
    }

    public DateOnly Date { get; private set; }

    public Payer Payer { get; private set; }

    public Guid CategoryId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public string? Comment { get; private set; }

    /// <summary>Set when the expense came from an import; a unique index makes a repeated import skip it.</summary>
    public string? ImportKey { get; private init; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<Expense> Create(
        DateOnly date,
        Payer payer,
        Guid categoryId,
        decimal amount,
        PaymentMethod paymentMethod,
        string? comment,
        DateTimeOffset now,
        string? importKey = null)
    {
        var check = EntryRules.Check(date, amount, comment, importKey);
        if (check.IsFailure)
            return check.Error!;

        return new Expense
        {
            Id = Guid.CreateVersion7(now),
            Date = date,
            Payer = payer,
            CategoryId = categoryId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            Comment = EntryRules.Normalize(comment),
            ImportKey = importKey,
            CreatedAtUtc = now,
        };
    }

    /// <summary>Replaces what the person can edit; the import key and creation time stay.</summary>
    public Result Update(
        DateOnly date, Payer payer, Guid categoryId, decimal amount, PaymentMethod paymentMethod, string? comment)
    {
        var check = EntryRules.Check(date, amount, comment, ImportKey);
        if (check.IsFailure)
            return check;

        Date = date;
        Payer = payer;
        CategoryId = categoryId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Comment = EntryRules.Normalize(comment);
        return Result.Success();
    }
}

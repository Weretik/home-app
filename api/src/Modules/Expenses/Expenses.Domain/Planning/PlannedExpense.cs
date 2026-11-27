using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace Expenses.Domain.Planning;

/// <summary>
/// An expense the household expects in a month but has not made yet (the «Прогноз» tab): a name, a category, who
/// pays and how much. It always counts toward the limits of its category in the forecast. When the money is spent,
/// the plan is recorded as a real <see cref="Expense"/> and disappears.
/// </summary>
public sealed class PlannedExpense : Entity<PlannedExpenseId>, IAggregateRoot
{
    public const int MaxNameLength = 100;

    private PlannedExpense()
    {
    }

    /// <summary>The first day of the month the plan belongs to.</summary>
    public DateOnly Month { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public CategoryId CategoryId { get; private set; }

    public Payer Payer { get; private set; }

    public decimal Amount { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<PlannedExpense> Create(
        DateOnly month, string name, CategoryId categoryId, Payer payer, decimal amount, DateTimeOffset now)
    {
        var plan = new PlannedExpense { Id = PlannedExpenseId.New(now), CreatedAtUtc = now };
        var set = plan.Update(month, name, categoryId, payer, amount);
        return set.IsFailure ? set.Error! : plan;
    }

    public Result Update(DateOnly month, string name, CategoryId categoryId, Payer payer, decimal amount)
    {
        var check = Result.Combine(
            Result.Ensure(month.Day == 1, PlannedExpenseErrors.MonthInvalid),
            Result.Ensure(!string.IsNullOrWhiteSpace(name), PlannedExpenseErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, PlannedExpenseErrors.NameTooLong),
            EntryRules.Check(month, amount, comment: null, importKey: null));
        if (check.IsFailure)
            return check;

        Month = month;
        Name = name!.Trim();
        CategoryId = categoryId;
        Payer = payer;
        Amount = amount;
        return Result.Success();
    }

    /// <summary>
    /// The real expense this plan turns into: the plan's payer, category and amount on <paramref name="date"/>,
    /// counted in the limits. The caller removes the plan in the same save.
    /// </summary>
    public Result<Expense> Record(DateOnly date, PaymentMethod paymentMethod, string? comment, DateTimeOffset now) =>
        Expense.Create(date, Payer, CategoryId, Amount, paymentMethod, comment, now);
}

using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Budget.Amounts;
using Expenses.Domain.Budget.Identifiers;
using Expenses.Domain.Categories;
using Expenses.Domain.Entries;

namespace Expenses.Domain.Budget;

/// <summary>
/// How much may be spent on a set of categories in a month or a quarter, like the limits of the old «Бюджет» sheet
/// («Еда и быт» is Продукты plus Быт). Limits may overlap: «Другое» also counts the categories that have their own
/// limit. The amount is kept by the period it starts in (<see cref="Amounts"/>), so a change applies from a chosen
/// period on and earlier periods keep theirs.
/// </summary>
public sealed class SpendingLimit : Entity<SpendingLimitId>, IAggregateRoot
{
    public const int MaxNameLength = 100;

    private List<CategoryId> _categoryIds = [];

    private SpendingLimit()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public LimitPeriod Period { get; private set; }

    /// <summary>The amounts by the period they start in; read one with <see cref="AmountFor"/>.</summary>
    public AmountHistory Amounts { get; private set; } = AmountHistory.Start(0);

    public IReadOnlyList<CategoryId> CategoryIds => _categoryIds;

    /// <summary>The position in lists, smallest first.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<SpendingLimit> Create(
        string name, LimitPeriod period, decimal amount, IReadOnlyCollection<CategoryId> categoryIds, int sortOrder, DateTimeOffset now)
    {
        var check = Check(name, amount, categoryIds);
        if (check.IsFailure)
            return check.Error!;

        var limit = new SpendingLimit { Id = SpendingLimitId.New(now) };
        limit.Apply(name, period, AmountHistory.Start(amount), categoryIds, sortOrder);
        return limit;
    }

    /// <summary>
    /// Replaces the name, period, categories and place, and sets <paramref name="amount"/> from the period that holds
    /// <paramref name="amountFrom"/> on: an explicit day always replaces the steps from that period on (R-LA-002), so a
    /// planned later change can be cancelled. Without a day the change applies from the period that holds
    /// <paramref name="today"/>, and an amount already in force there leaves the steps alone (R-LA-005), so a rename
    /// keeps planned changes. A change between month and quarter starts the history again, since month steps mean
    /// nothing for quarters.
    /// </summary>
    public Result Update(
        string name,
        LimitPeriod period,
        decimal amount,
        IReadOnlyCollection<CategoryId> categoryIds,
        int sortOrder,
        DateOnly? amountFrom,
        DateOnly today)
    {
        var check = Result.Combine(
            Check(name, amount, categoryIds),
            Result.Ensure(AmountHistory.Covers(amountFrom ?? today), BudgetErrors.AmountFromOutOfRange));
        if (check.IsFailure)
            return check;

        var from = LimitCarryOver.PeriodStart(period, amountFrom ?? today);
        var amounts = period != Period
            ? AmountHistory.Start(amount)
            : amountFrom is null && Amounts.For(from) == amount ? Amounts : Amounts.ChangeFrom(from, amount);
        Apply(name, period, amounts, categoryIds, sortOrder);
        return Result.Success();
    }

    /// <summary>The amount of the period that starts on <paramref name="periodStart"/>.</summary>
    public decimal AmountFor(DateOnly periodStart) => Amounts.For(periodStart);

    /// <summary>The first day of the month or quarter that contains <paramref name="day"/>, and the day after its end.</summary>
    public (DateOnly Start, DateOnly End) PeriodAround(DateOnly day)
    {
        var start = LimitCarryOver.PeriodStart(Period, day);
        return (start, start.AddMonths(LimitCarryOver.Months(Period)));
    }

    private static Result Check(string name, decimal amount, IReadOnlyCollection<CategoryId> categoryIds) =>
        Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name), BudgetErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, BudgetErrors.NameTooLong),
            Result.Ensure(IsMoney(amount) && amount > 0, BudgetErrors.AmountInvalid),
            Result.Ensure(
                categoryIds is { Count: > 0 } && categoryIds.Distinct().Count() == categoryIds.Count && !categoryIds.Contains(default),
                BudgetErrors.CategoriesRequired));

    private void Apply(string name, LimitPeriod period, AmountHistory amounts, IReadOnlyCollection<CategoryId> categoryIds, int sortOrder)
    {
        Name = name.Trim();
        Period = period;
        Amounts = amounts;
        _categoryIds = [.. categoryIds];
        SortOrder = sortOrder;
    }

    internal static bool IsMoney(decimal amount) =>
        amount >= 0 && amount <= EntryRules.MaxAmount && decimal.Round(amount, 2) == amount;
}

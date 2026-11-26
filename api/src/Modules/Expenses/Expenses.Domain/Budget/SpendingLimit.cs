using BuildingBlocks.Domain.Modeling;
using BuildingBlocks.Domain.Results;
using Expenses.Domain.Entries;

namespace Expenses.Domain.Budget;

/// <summary>
/// How much may be spent on a set of categories in a month or a quarter, like the limits of the old «Бюджет» sheet
/// («Еда и быт» is Продукты plus Быт). Limits may overlap: «Другое» also counts the categories that have their own
/// limit.
/// </summary>
public sealed class SpendingLimit : Entity<Guid>, IAggregateRoot
{
    public const int MaxNameLength = 100;

    private List<Guid> _categoryIds = [];

    private SpendingLimit()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public LimitPeriod Period { get; private set; }

    public decimal Amount { get; private set; }

    public IReadOnlyList<Guid> CategoryIds => _categoryIds;

    /// <summary>The position in lists, smallest first.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Changes on every save (PostgreSQL <c>xmin</c>); an edit based on an older version is refused.</summary>
    public uint Version { get; private set; }

    public static Result<SpendingLimit> Create(
        string name, LimitPeriod period, decimal amount, IReadOnlyCollection<Guid> categoryIds, int sortOrder, DateTimeOffset now)
    {
        var limit = new SpendingLimit { Id = Guid.CreateVersion7(now) };
        var set = limit.Update(name, period, amount, categoryIds, sortOrder);
        return set.IsFailure ? set.Error! : limit;
    }

    public Result Update(string name, LimitPeriod period, decimal amount, IReadOnlyCollection<Guid> categoryIds, int sortOrder)
    {
        var check = Result.Combine(
            Result.Ensure(!string.IsNullOrWhiteSpace(name), BudgetErrors.NameRequired),
            Result.Ensure(name is null || name.Trim().Length <= MaxNameLength, BudgetErrors.NameTooLong),
            Result.Ensure(IsMoney(amount) && amount > 0, BudgetErrors.AmountInvalid),
            Result.Ensure(
                categoryIds is { Count: > 0 } && categoryIds.Distinct().Count() == categoryIds.Count && !categoryIds.Contains(Guid.Empty),
                BudgetErrors.CategoriesRequired));
        if (check.IsFailure)
            return check;

        Name = name!.Trim();
        Period = period;
        Amount = amount;
        _categoryIds = [.. categoryIds];
        SortOrder = sortOrder;
        return Result.Success();
    }

    /// <summary>The first day of the month or quarter that contains <paramref name="day"/>, and the day after its end.</summary>
    public (DateOnly Start, DateOnly End) PeriodAround(DateOnly day)
    {
        var firstMonth = Period == LimitPeriod.Quarter ? ((day.Month - 1) / 3 * 3) + 1 : day.Month;
        var start = new DateOnly(day.Year, firstMonth, 1);
        return (start, start.AddMonths(Period == LimitPeriod.Quarter ? 3 : 1));
    }

    internal static bool IsMoney(decimal amount) =>
        amount >= 0 && amount <= EntryRules.MaxAmount && decimal.Round(amount, 2) == amount;
}

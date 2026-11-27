namespace Expenses.Domain.Budget.Amounts;

/// <summary>
/// The amounts of a limit by the period they start in, so changing a limit does not rewrite earlier periods
/// (ADR 0041). Immutable: a change returns a new history. The amount of a period is the amount of the last step that
/// starts on or before the period's first day; a period before the first step takes the first step's amount.
/// </summary>
public sealed class AmountHistory : IEquatable<AmountHistory>
{
    /// <summary>The start of the first step of a new limit, so its amount applies to every period.</summary>
    public static readonly DateOnly Beginning = new(2000, 1, 1);

    /// <summary>The last day a step may start on.</summary>
    public static readonly DateOnly LastDay = new(2100, 12, 31);

    private readonly AmountStep[] _steps;

    private AmountHistory(AmountStep[] steps) => _steps = steps;

    /// <summary>The steps, oldest first, one per start day; never empty. A read-only view, so the history stays immutable.</summary>
    public IReadOnlyList<AmountStep> Steps => Array.AsReadOnly(_steps);

    /// <summary>One amount for every period: a single step from <see cref="Beginning"/>.</summary>
    public static AmountHistory Start(decimal amount) => new([new AmountStep(Beginning, amount)]);

    /// <summary>
    /// A history over stored steps, put in order; of steps that start on the same day the last one given wins, so a
    /// period never has two amounts. A history always has at least one step: an empty list is a programmer error
    /// (storage maps an empty column itself, see the Infrastructure converter).
    /// </summary>
    public static AmountHistory From(IEnumerable<AmountStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        var ordered = steps
            .Select((step, index) => (step, index))
            .GroupBy(x => x.step.From)
            .Select(g => g.MaxBy(x => x.index).step)
            .OrderBy(s => s.From)
            .ToArray();
        ArgumentOutOfRangeException.ThrowIfZero(ordered.Length, nameof(steps));
        return new AmountHistory(ordered);
    }

    /// <summary>Whether a step may start on <paramref name="day"/>.</summary>
    public static bool Covers(DateOnly day) => day >= Beginning && day <= LastDay;

    /// <summary>The amount of the period that starts on <paramref name="periodStart"/>.</summary>
    public decimal For(DateOnly periodStart)
    {
        var amount = _steps[0].Amount;
        foreach (var step in _steps)
        {
            if (step.From > periodStart)
                break;
            amount = step.Amount;
        }

        return amount;
    }

    /// <summary>
    /// <paramref name="amount"/> from the period that starts on <paramref name="from"/> onward: the steps that start on
    /// or after it are replaced by one step, which is left out when the period before already has that amount.
    /// </summary>
    public AmountHistory ChangeFrom(DateOnly from, decimal amount)
    {
        var kept = _steps.Where(s => s.From < from).ToList();
        if (kept.Count == 0 || kept[^1].Amount != amount)
            kept.Add(new AmountStep(from, amount));
        return new AmountHistory([.. kept]);
    }

    public bool Equals(AmountHistory? other) => other is not null && _steps.SequenceEqual(other._steps);

    public override bool Equals(object? obj) => Equals(obj as AmountHistory);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var step in _steps)
            hash.Add(step);
        return hash.ToHashCode();
    }
}

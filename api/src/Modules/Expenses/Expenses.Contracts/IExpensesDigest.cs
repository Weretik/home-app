namespace Expenses.Contracts;

/// <summary>What the Notifications module may read from Expenses; implemented by the Expenses module.</summary>
public interface IExpensesDigest
{
    /// <summary>The expenses of the days <paramref name="from"/> to <paramref name="to"/>, both included.</summary>
    Task<ExpensesPeriodReport> GetPeriodAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);

    /// <summary>The month summary (the same figures as the site's «Сводка»).</summary>
    Task<ExpensesMonthReport> GetMonthAsync(int year, int month, CancellationToken cancellationToken);
}

using Expenses.Application.Features.Summary.Report;
using Expenses.Contracts;
using Expenses.Domain.Budget;

namespace Expenses.Application.Features.Summary.Digest;

/// <summary>The month summary as the contract's <see cref="ExpensesMonthReport"/>; pure, so it is unit-tested.</summary>
public static class DigestReports
{
    public static ExpensesMonthReport ToReport(MonthSummary s) => new(
        s.Year,
        s.Month,
        s.Incomes,
        [.. s.IncomesByPerson.Select(p => new NamedAmount(p.Person.ToString(), p.Amount))],
        s.Expenses,
        [.. s.ExpensesByPayer.Select(p => new NamedAmount(p.Payer.ToString(), p.Amount))],
        s.MonthlyLimit,
        s.CarriedIn,
        s.Result,
        s.SavingsNet,
        s.SavingsBalance,
        s.SavingsGoal,
        [.. s.Limits.Select(l => new LimitState(l.Name, l.Period == LimitPeriod.Quarter, l.Amount, l.Spent + l.CarriedIn, l.Left, l.Percent))],
        [.. s.TopCategories.Select(c => new NamedAmount(c.Name, c.Amount))]);

    public static ExpensesMonthReport Empty(int year, int month) =>
        new(year, month, 0, [], 0, [], 0, 0, 0, 0, 0, 0, [], []);
}

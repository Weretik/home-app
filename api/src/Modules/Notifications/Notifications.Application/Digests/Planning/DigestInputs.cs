using Expenses.Contracts;
using Gifts.Contracts;
using Shopping.Contracts;
using Todos.Contracts;

namespace Notifications.Application.Digests.Planning;

/// <summary>What the due digests need from the other modules; a part is null when no due digest needs it.</summary>
/// <param name="WeekMonth">The month so far, shown under the weekly summary.</param>
public sealed record DigestInputs(
    IReadOnlyList<OpenTodo>? Todos = null,
    IReadOnlyList<GiftPurchase>? Gifts = null,
    IReadOnlyList<NeededItem>? Shopping = null,
    ExpensesPeriodReport? Week = null,
    ExpensesMonthReport? WeekMonth = null,
    ExpensesMonthReport? Month = null);

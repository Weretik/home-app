using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Planning;

public static class PlannedExpenseErrors
{
    public static readonly Error NameRequired =
        Error.Validation("expenses.planned_name_required", "The planned expense needs a name.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("expenses.planned_name_too_long", $"The name is longer than {PlannedExpense.MaxNameLength} characters.", "name");

    public static readonly Error MonthInvalid =
        Error.Validation("expenses.planned_month_invalid", "A plan belongs to a month: the first day of a month between 2000 and 2100.", "month");
}

using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Planning;

public static class PlannedExpenseErrors
{
    public static readonly Error NameRequired =
        Error.Validation("expenses.planned_name_required", "Запланованій витраті потрібна назва.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("expenses.planned_name_too_long", $"Назва задовга: не більше {PlannedExpense.MaxNameLength} символів.", "name");

    public static readonly Error MonthInvalid =
        Error.Validation("expenses.planned_month_invalid", "План належить до місяця: перший день місяця між 2000 і 2100 роками.", "month");
}

using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Budget;

public static class BudgetErrors
{
    public static readonly Error NameRequired =
        Error.Validation("expenses.limit_name_required", "Ліміту потрібна назва.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("expenses.limit_name_too_long", $"Назва ліміту задовга: не більше {SpendingLimit.MaxNameLength} символів.", "name");

    public static readonly Error AmountInvalid =
        Error.Validation("expenses.limit_amount_invalid", "Ліміт — додатна сума в гривнях, не більше двох знаків після коми.", "amount");

    public static readonly Error CategoriesRequired =
        Error.Validation("expenses.limit_categories_required", "Ліміт охоплює щонайменше одну категорію, кожну один раз.", "categoryIds");

    public static readonly Error SettingsInvalid =
        Error.Validation("expenses.budget_settings_invalid", "Загальний ліміт і ціль скарбнички — суми від 0, не більше двох знаків після коми.");

    public static readonly Error LimitsFromOutOfRange =
        Error.Validation("expenses.limits_from_out_of_range", "День, з якого рахуються ліміти, має бути між 2000-01-01 і 2100-12-31.", "limitsFrom");

    public static readonly Error AmountFromOutOfRange =
        Error.Validation("expenses.amount_from_out_of_range", "Місяць, з якого діє сума ліміту, має бути між 2000-01-01 і 2100-12-31.", "amountFrom");

    public static readonly Error MonthlyLimitFromOutOfRange =
        Error.Validation("expenses.monthly_limit_from_out_of_range", "Місяць, з якого діє загальний ліміт, має бути між 2000-01-01 і 2100-12-31.", "monthlyLimitFrom");

    /// <summary>
    /// The one settings row is part of the schema (inserted by the <c>Budget</c> migration); without it the budget can
    /// be neither shown nor saved.
    /// </summary>
    public static readonly Error SettingsNotFound =
        Error.NotFound("expenses.budget_settings_not_found", "Немає рядка налаштувань бюджету; відновіть його з міграції Budget.");

    /// <summary>Two limits were added at the same moment and got the same place in the list.</summary>
    public static readonly Error LimitAddedMeanwhile =
        Error.Conflict("expenses.limit_added_meanwhile", "Саме в цю мить додали інший ліміт. Спробуйте ще раз.");

    public static Error LimitNotFound(Guid id) =>
        Error.NotFound("expenses.limit_not_found", $"Ліміту {id} немає.");
}

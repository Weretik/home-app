using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Budget;

public static class BudgetErrors
{
    public static readonly Error NameRequired =
        Error.Validation("expenses.limit_name_required", "The limit needs a name.", "name");

    public static readonly Error NameTooLong =
        Error.Validation("expenses.limit_name_too_long", $"The limit name is longer than {SpendingLimit.MaxNameLength} characters.", "name");

    public static readonly Error AmountInvalid =
        Error.Validation("expenses.limit_amount_invalid", "A limit is a positive amount in hryvnias with at most two digits after the point.", "amount");

    public static readonly Error CategoriesRequired =
        Error.Validation("expenses.limit_categories_required", "A limit covers at least one category, each once.", "categoryIds");

    public static readonly Error SettingsInvalid =
        Error.Validation("expenses.budget_settings_invalid", "The total limit and the savings goal are amounts from 0 with at most two digits after the point.");

    public static readonly Error LimitsFromOutOfRange =
        Error.Validation("expenses.limits_from_out_of_range", "The day limits start counting must be between 2000-01-01 and 2100-12-31.", "limitsFrom");

    /// <summary>
    /// The one settings row is part of the schema (inserted by the <c>Budget</c> migration); without it the budget can
    /// be neither shown nor saved.
    /// </summary>
    public static readonly Error SettingsNotFound =
        Error.NotFound("expenses.budget_settings_not_found", "The budget settings row is missing; restore it from the Budget migration.");

    /// <summary>Two limits were added at the same moment and got the same place in the list.</summary>
    public static readonly Error LimitAddedMeanwhile =
        Error.Conflict("expenses.limit_added_meanwhile", "Another limit was added at the same moment. Try again.");

    public static Error LimitNotFound(Guid id) =>
        Error.NotFound("expenses.limit_not_found", $"There is no spending limit {id}.");
}

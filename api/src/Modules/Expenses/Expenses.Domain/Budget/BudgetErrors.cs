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

    public static Error LimitNotFound(Guid id) =>
        Error.NotFound("expenses.limit_not_found", $"There is no spending limit {id}.");
}

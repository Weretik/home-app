using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Entries;

/// <summary>The rules every money entry (expense or income) breaks the same way.</summary>
public static class EntryErrors
{
    public static readonly Error AmountNotPositive =
        Error.Validation("expenses.amount_not_positive", "The amount must be greater than zero.", "amount");

    public static readonly Error AmountTooLarge =
        Error.Validation("expenses.amount_too_large", $"The amount must not exceed {EntryRules.MaxAmount:0}.", "amount");

    public static readonly Error AmountTooPrecise =
        Error.Validation("expenses.amount_too_precise", "The amount has at most two digits after the point.", "amount");

    public static readonly Error DateOutOfRange =
        Error.Validation("expenses.date_out_of_range", "The date must be between 2000-01-01 and 2100-12-31.", "date");

    public static readonly Error CommentTooLong =
        Error.Validation("expenses.comment_too_long", $"The comment is longer than {EntryRules.MaxCommentLength} characters.", "comment");

    public static readonly Error ImportKeyTooLong =
        Error.Validation("expenses.import_key_too_long", $"The import key is longer than {EntryRules.MaxImportKeyLength} characters.", "importKey");

    public static readonly Error AmountZero =
        Error.Validation("expenses.amount_zero", "The amount must not be zero.", "amount");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "expenses.changed_meanwhile", "Someone changed or deleted this entry in the meantime; reload and try again.");

    public static Error NotFound(string what, Guid id) =>
        Error.NotFound($"expenses.{what}_not_found", $"There is no {what.Replace('_', ' ')} {id}.");
}

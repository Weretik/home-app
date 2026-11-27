using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Entries;

/// <summary>The rules every money entry (expense or income) breaks the same way.</summary>
public static class EntryErrors
{
    public static readonly Error AmountNotPositive =
        Error.Validation("expenses.amount_not_positive", "Сума має бути більшою за нуль.", "amount");

    public static readonly Error AmountTooLarge =
        Error.Validation("expenses.amount_too_large", $"Сума не може перевищувати {EntryRules.MaxAmount:0}.", "amount");

    public static readonly Error AmountTooPrecise =
        Error.Validation("expenses.amount_too_precise", "У сумі не більше двох знаків після коми.", "amount");

    public static readonly Error DateOutOfRange =
        Error.Validation("expenses.date_out_of_range", "Дата має бути між 2000-01-01 і 2100-12-31.", "date");

    public static readonly Error CommentTooLong =
        Error.Validation("expenses.comment_too_long", $"Коментар задовгий: не більше {EntryRules.MaxCommentLength} символів.", "comment");

    public static readonly Error ImportKeyTooLong =
        Error.Validation("expenses.import_key_too_long", $"Ключ імпорту задовгий: не більше {EntryRules.MaxImportKeyLength} символів.", "importKey");

    public static readonly Error AmountZero =
        Error.Validation("expenses.amount_zero", "Сума не може дорівнювати нулю.", "amount");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "expenses.changed_meanwhile", "Тим часом цей запис хтось змінив або видалив; оновіть сторінку й спробуйте ще раз.");

    public static Error NotFound(string what, Guid id) =>
        Error.NotFound($"expenses.{what}_not_found", $"Немає {Genitive(what)} {id}.");

    /// <summary>The record kind in the genitive, for <see cref="NotFound"/>.</summary>
    private static string Genitive(string what) => what switch
    {
        "expense" => "витрати",
        "income" => "доходу",
        "planned_expense" => "запланованої витрати",
        "savings_movement" => "запису скарбнички",
        _ => "запису",
    };
}

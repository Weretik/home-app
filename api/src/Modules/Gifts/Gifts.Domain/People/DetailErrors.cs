using BuildingBlocks.Domain.Results;

namespace Gifts.Domain.People;

public static class DetailErrors
{
    public static readonly Error LabelRequired =
        Error.Validation("gifts.detail_label_required", "The detail needs a label, such as «Кольцо».", "label");

    public static readonly Error LabelTooLong =
        Error.Validation("gifts.detail_label_too_long", $"The label is longer than {Detail.MaxLabelLength} characters.", "label");

    public static readonly Error ValueRequired =
        Error.Validation("gifts.detail_value_required", "The detail needs a value, such as «18,5».", "value");

    public static readonly Error ValueTooLong =
        Error.Validation("gifts.detail_value_too_long", $"The value is longer than {Detail.MaxValueLength} characters.", "value");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "gifts.changed_meanwhile", "Someone changed or deleted this detail in the meantime; reload and try again.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("gifts.detail_not_found", $"There is no detail {id}.");
}

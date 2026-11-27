using BuildingBlocks.Domain.Results;

namespace Gifts.Domain.People;

public static class DetailErrors
{
    public static readonly Error LabelRequired =
        Error.Validation("gifts.detail_label_required", "Нюансу потрібна назва, наприклад «Каблучка».", "label");

    public static readonly Error LabelTooLong =
        Error.Validation("gifts.detail_label_too_long", $"Назва задовга: не більше {Detail.MaxLabelLength} символів.", "label");

    public static readonly Error ValueRequired =
        Error.Validation("gifts.detail_value_required", "Нюансу потрібне значення, наприклад «18,5».", "value");

    public static readonly Error ValueTooLong =
        Error.Validation("gifts.detail_value_too_long", $"Значення задовге: не більше {Detail.MaxValueLength} символів.", "value");

    public static readonly Error ChangedMeanwhile = Error.Conflict(
        "gifts.changed_meanwhile", "Тим часом цей нюанс хтось змінив або видалив; оновіть сторінку й спробуйте ще раз.");

    public static Error NotFound(Guid id) =>
        Error.NotFound("gifts.detail_not_found", $"Нюансу {id} немає.");
}

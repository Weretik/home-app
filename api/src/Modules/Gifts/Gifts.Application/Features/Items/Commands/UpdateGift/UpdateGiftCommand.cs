using BuildingBlocks.Domain.Results;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Mediator;

namespace Gifts.Application.Features.Items.Commands.UpdateGift;

/// <summary>
/// Replaces the editable fields of a gift and its whole variant list, if nobody changed it since
/// <see cref="Version"/>. The status stays.
/// </summary>
public sealed record UpdateGiftCommand(
    Guid Id,
    uint Version,
    Recipient Recipient,
    string Name,
    Guid CategoryId,
    Priority Priority,
    decimal? Price,
    string? Link,
    string? Occasion,
    DateOnly? OccasionDate,
    string? Notes,
    IReadOnlyList<VariantDraft> Variants) : ICommand<Result>;

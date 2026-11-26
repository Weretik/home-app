using BuildingBlocks.Domain.Results;
using Gifts.Application.Idempotency;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;
using Mediator;

namespace Gifts.Application.Features.Items.Commands.CreateGift;

/// <summary>Adds a gift idea to the active list. A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreateGiftCommand(
    string IdempotencyKey,
    Recipient Recipient,
    string Name,
    Guid CategoryId,
    Priority Priority,
    decimal? Price,
    string? Link,
    string? Occasion,
    DateOnly? OccasionDate,
    string? Notes,
    IReadOnlyList<VariantDraft> Variants) : ICommand<Result<CreatedGiftsEntry>>;

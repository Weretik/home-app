using BuildingBlocks.Domain.Results;
using Gifts.Application.Idempotency;
using Gifts.Domain.People;
using Mediator;

namespace Gifts.Application.Features.Details.Commands.CreateDetail;

/// <summary>Adds a detail at the end of the recipient's list. A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreateDetailCommand(string IdempotencyKey, Recipient Recipient, string Label, string Value)
    : ICommand<Result<CreatedGiftsEntry>>;

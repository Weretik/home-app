using BuildingBlocks.Domain.Results;
using Mediator;

namespace Gifts.Application.Features.Items.Commands.DeleteGift;

/// <summary>Deletes the gift and its variants for good.</summary>
public sealed record DeleteGiftCommand(Guid Id) : ICommand<Result>;

using BuildingBlocks.Domain.Results;
using Gifts.Domain.Wishlist;
using Mediator;

namespace Gifts.Application.Features.Items.Commands.ChangeGiftStatus;

/// <summary>
/// «Подарено», «Не актуально» or «Вернуть в список»: sets the status (a target value, so a repeat changes nothing), if
/// nobody changed the gift since <see cref="Version"/>. <see cref="GiftedOn"/> counts only for
/// <see cref="GiftStatus.Gifted"/>.
/// </summary>
public sealed record ChangeGiftStatusCommand(Guid Id, uint Version, GiftStatus Status, DateOnly? GiftedOn) : ICommand<Result>;

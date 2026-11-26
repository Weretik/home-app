using BuildingBlocks.Domain.Results;
using Mediator;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Commands.SetItemStatus;

/// <summary>Ticks an item bought or needed. A target value, so a repeat changes nothing; no version needed.</summary>
public sealed record SetItemStatusCommand(Guid Id, ShoppingItemStatus Status, Person By) : ICommand<Result<ShoppingItemView>>;

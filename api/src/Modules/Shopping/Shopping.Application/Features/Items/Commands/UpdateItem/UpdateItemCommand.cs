using BuildingBlocks.Domain.Results;
using Mediator;
using Shopping.Application.Features.Items.Views;

namespace Shopping.Application.Features.Items.Commands.UpdateItem;

/// <summary>
/// The edit dialog: replaces the item's fields if nobody changed it since <see cref="Version"/>. Moving it to another
/// department (not the fallback) teaches the dictionary its name.
/// </summary>
public sealed record UpdateItemCommand(Guid Id, uint Version, string Name, string? Quantity, string? Note, Guid DepartmentId, bool Urgent)
    : ICommand<Result<ShoppingItemView>>;

using Shopping.Application.Features.Items.Views;

namespace Shopping.Application.Features.Items.Commands.AddItems;

/// <summary>What an add did: new rows, items brought back from the cart, and needed items it updated.</summary>
public sealed record AddItemsResult(
    IReadOnlyList<ShoppingItemView> Added,
    IReadOnlyList<ShoppingItemView> Restored,
    IReadOnlyList<ShoppingItemView> Updated);

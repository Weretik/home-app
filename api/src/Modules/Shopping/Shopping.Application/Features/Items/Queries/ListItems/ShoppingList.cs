using Shopping.Application.Features.Items.Views;

namespace Shopping.Application.Features.Items.Queries.ListItems;

/// <summary>Needed items (by department in store order, urgent first, then by when added), then the cart (latest bought first).</summary>
public sealed record ShoppingList(IReadOnlyList<ShoppingItemView> Items);

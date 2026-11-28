namespace Shopping.Domain.Items.Lifecycle;

/// <summary>«Нужно» or «Куплено». Whether a bought item is still in the cart or already history is computed (<see cref="CartWindow"/>).</summary>
public enum ShoppingItemStatus
{
    Needed,
    Bought,
}

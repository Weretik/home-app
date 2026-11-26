using System.Linq.Expressions;

namespace Shopping.Domain.Items;

/// <summary>
/// Where an item is, computed from its data and the current time, never stored: it is in the list while needed, or
/// while bought, not cleared and bought less than <c>CartKeepHours</c> ago (the cart); every other bought item is
/// history. The expressions are the same rule for queries, so SQL and the domain cannot disagree.
/// </summary>
public static class CartWindow
{
    public static DateTimeOffset Cutoff(DateTimeOffset now, int cartKeepHours) => now.AddHours(-cartKeepHours);

    public static bool IsInList(ShoppingItem item, DateTimeOffset cutoff) =>
        item.Status == ShoppingItemStatus.Needed || IsInCart(item, cutoff);

    public static bool IsInCart(ShoppingItem item, DateTimeOffset cutoff) =>
        item.Status == ShoppingItemStatus.Bought && item.ClearedAtUtc == null && item.BoughtAtUtc > cutoff;

    public static Expression<Func<ShoppingItem, bool>> InList(DateTimeOffset cutoff) =>
        i => i.Status == ShoppingItemStatus.Needed
            || (i.Status == ShoppingItemStatus.Bought && i.ClearedAtUtc == null && i.BoughtAtUtc > cutoff);

    public static Expression<Func<ShoppingItem, bool>> InCart(DateTimeOffset cutoff) =>
        i => i.Status == ShoppingItemStatus.Bought && i.ClearedAtUtc == null && i.BoughtAtUtc > cutoff;

    public static Expression<Func<ShoppingItem, bool>> InHistory(DateTimeOffset cutoff) =>
        i => i.Status == ShoppingItemStatus.Bought && (i.ClearedAtUtc != null || i.BoughtAtUtc <= cutoff);
}

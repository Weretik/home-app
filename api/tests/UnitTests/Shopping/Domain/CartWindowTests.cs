using Shopping.Domain.Departments;
using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;

namespace UnitTests.Shopping.Domain;

/// <summary>List, cart and history are computed from the item and the time (default window: 3 hours).</summary>
public sealed class CartWindowTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 18, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Cutoff = CartWindow.Cutoff(Now, 3);

    [Fact]
    public void A_NeededItem_IsAlwaysInTheList()
    {
        var item = Bought(null);

        Assert.True(CartWindow.IsInList(item, Cutoff));
        Assert.True(InList(item));
        Assert.False(InHistory(item));
    }

    [Fact]
    public void An_ItemBought_2HoursAgo_IsInTheCart()
    {
        var item = Bought(Now.AddHours(-2));

        Assert.True(CartWindow.IsInCart(item, Cutoff));
        Assert.True(InList(item));
        Assert.False(InHistory(item));
    }

    [Fact]
    public void An_ItemBought_4HoursAgo_IsHistory()
    {
        var item = Bought(Now.AddHours(-4));

        Assert.False(CartWindow.IsInList(item, Cutoff));
        Assert.False(InList(item));
        Assert.True(InHistory(item));
    }

    [Fact]
    public void An_ItemBought_ExactlyAtTheCutoff_IsHistory()
    {
        var item = Bought(Cutoff);

        Assert.False(CartWindow.IsInList(item, Cutoff));
        Assert.True(InHistory(item));
    }

    [Fact]
    public void A_LongerWindow_KeepsItInTheCart()
    {
        var item = Bought(Now.AddHours(-4));

        Assert.True(CartWindow.IsInList(item, CartWindow.Cutoff(Now, 5)));
    }

    private static bool InList(ShoppingItem item) => CartWindow.InList(Cutoff).Compile()(item);

    private static bool InHistory(ShoppingItem item) => CartWindow.InHistory(Cutoff).Compile()(item);

    private static ShoppingItem Bought(DateTimeOffset? boughtAt)
    {
        var item = ShoppingItem.Create("Хлеб", null, null, new DepartmentId(Guid.NewGuid()), false, Person.Vitaliy, Now.AddDays(-1)).Value;
        if (boughtAt is { } at)
            item.SetStatus(ShoppingItemStatus.Bought, Person.Maria, at);
        return item;
    }
}

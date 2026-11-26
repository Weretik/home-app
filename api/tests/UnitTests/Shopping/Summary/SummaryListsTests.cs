using Shopping.Application.Features.Items.Queries.GetSummary;
using Shopping.Application.Features.Items.Views;
using static UnitTests.Shopping.Summary.SummaryRows;

namespace UnitTests.Shopping.Summary;

/// <summary>The summary's lists: departments with needed items in store order, and the next items, urgent first.</summary>
public sealed class SummaryListsTests
{
    [Fact]
    public void Departments_CountOnlyNeededItems_InStoreOrder()
    {
        ShoppingItemView[] list = [Item("кефир", Dairy), Item("лук", Vegetables), Item("сыр", Dairy), Item("огурцы", Vegetables, bought: true)];

        var departments = SummaryFigures.Build(list, 0, 0, [], 8).Departments;

        Assert.Equal([("Овощи", 1), ("Молочное", 2)], departments.Select(d => (d.Name, d.NeededCount)));
    }

    [Fact]
    public void Next_Items_AreUrgentFirst_KeepTheListOrder_AndStopAtTheCount()
    {
        ShoppingItemView[] list = [Item("1", Vegetables), Item("2", Vegetables, urgent: true), Item("3", Dairy), Item("4", Dairy, urgent: true)];

        var next = SummaryFigures.Build(list, 0, 0, [], 3).NextItems;

        Assert.Equal(["2", "4", "1"], next.Select(v => v.Name));
    }
}

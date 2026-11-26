using Shopping.Application.Features.Items.Queries.GetSummary;
using Shopping.Application.Features.Items.Views;
using static UnitTests.Shopping.Summary.SummaryRows;

namespace UnitTests.Shopping.Summary;

/// <summary>The summary's figures: needed, urgent and in-cart counts, the bought counts and the most bought names.</summary>
public sealed class SummaryCountsTests
{
    [Fact]
    public void Counts_SplitTheList_IntoNeeded_Urgent_AndTheCart()
    {
        ShoppingItemView[] list =
        [
            Item("морковь", Vegetables, urgent: true),
            Item("лук", Vegetables),
            Item("молоко", Dairy, bought: true),
        ];
        FrequentName[] top = [new("хлеб", Dairy.Id, 12, DateTimeOffset.UnixEpoch)];

        var summary = SummaryFigures.Build(list, 4, 9, top, 8);

        Assert.Equal((2, 1, 1), (summary.NeededCount, summary.UrgentCount, summary.InCartCount));
        Assert.Equal((4, 9), (summary.BoughtLast7Days, summary.BoughtLast30Days));
        Assert.Equal(new SummaryFrequentItem("хлеб", 12), Assert.Single(summary.TopFrequent));
    }

    [Fact]
    public void An_Empty_List_HasNoDepartments_AndNoNextItems()
    {
        var summary = SummaryFigures.Build([], 0, 0, [], 8);

        Assert.Equal(0, summary.NeededCount);
        Assert.Empty(summary.Departments);
        Assert.Empty(summary.NextItems);
    }
}

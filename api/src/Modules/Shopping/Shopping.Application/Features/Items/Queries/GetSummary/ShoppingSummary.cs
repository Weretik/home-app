using Shopping.Application.Features.Items.Views;

namespace Shopping.Application.Features.Items.Queries.GetSummary;

/// <summary>Counts, departments with needed items in store order, the next items to buy and the top frequent names.</summary>
public sealed record ShoppingSummary(
    int NeededCount,
    int UrgentCount,
    int InCartCount,
    int BoughtLast7Days,
    int BoughtLast30Days,
    IReadOnlyList<SummaryDepartment> Departments,
    IReadOnlyList<ShoppingItemView> NextItems,
    IReadOnlyList<SummaryFrequentItem> TopFrequent);

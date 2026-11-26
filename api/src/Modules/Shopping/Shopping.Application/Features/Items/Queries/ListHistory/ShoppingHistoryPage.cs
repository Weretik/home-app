using Shopping.Application.Features.Items.Views;

namespace Shopping.Application.Features.Items.Queries.ListHistory;

/// <summary>One page of history, with the count of all history rows.</summary>
public sealed record ShoppingHistoryPage(IReadOnlyList<ShoppingItemView> Items, int Page, int PageSize, int TotalCount);

using Shopping.Domain.Items;
using Shopping.Domain.Items.Lifecycle;

namespace Shopping.Application.Features.Items.Views;

/// <summary>An item as the list, the cart, history and the summary show it, with its department.</summary>
public sealed record ShoppingItemView(
    Guid Id,
    string Name,
    string? Quantity,
    string? Note,
    Guid DepartmentId,
    string DepartmentName,
    string DepartmentIcon,
    int DepartmentSortOrder,
    bool Urgent,
    ShoppingItemStatus Status,
    Person AddedBy,
    DateTimeOffset AddedAtUtc,
    Person? BoughtBy,
    DateTimeOffset? BoughtAtUtc,
    uint Version);

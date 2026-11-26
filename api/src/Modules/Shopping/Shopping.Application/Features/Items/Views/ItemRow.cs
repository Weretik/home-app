using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Views;

/// <summary>An item joined with its department in a query, so it can still be filtered, ordered and paged in SQL.</summary>
public sealed class ItemRow
{
    public required ShoppingItem Item { get; init; }

    public required Department Department { get; init; }
}

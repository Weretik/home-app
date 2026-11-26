using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Items;

namespace UnitTests.Shopping.Summary;

/// <summary>List rows for the summary tests: a needed or bought item in a department.</summary>
internal static class SummaryRows
{
    private static readonly DateTimeOffset Added = new(2026, 11, 26, 9, 0, 0, TimeSpan.Zero);

    public static readonly (Guid Id, string Name, string Icon, int Order) Vegetables = (Guid.NewGuid(), "Овощи", "🥦", 10);
    public static readonly (Guid Id, string Name, string Icon, int Order) Dairy = (Guid.NewGuid(), "Молочное", "🥛", 20);

    public static ShoppingItemView Item(
        string name, (Guid Id, string Name, string Icon, int Order) department, bool urgent = false, bool bought = false) =>
        new(Guid.NewGuid(), name, null, null, department.Id, department.Name, department.Icon, department.Order, urgent,
            bought ? ShoppingItemStatus.Bought : ShoppingItemStatus.Needed, Person.Maria, Added,
            bought ? Person.Vitaliy : null, bought ? Added.AddHours(1) : null, 1);
}

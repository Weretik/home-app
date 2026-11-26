using System.Text;
using Shopping.Contracts;

namespace Notifications.Application.Digests.Texts;

/// <summary>The evening shopping list (spec notifications/001, R-011). Null when nothing is needed.</summary>
public static class ShoppingText
{
    public static string? For(IReadOnlyList<NeededItem> items, string shoppingUrl)
    {
        if (items.Count == 0)
            return null;

        var text = new StringBuilder($"🛒 <b>В списке покупок: {items.Count}</b>\n");
        var lines = new List<string>();
        foreach (var department in items.GroupBy(i => (i.Department, i.DepartmentIcon)))
        {
            lines.Add($"\n{department.Key.DepartmentIcon} <b>{TextFormat.Escape(department.Key.Department)}</b>");
            lines.AddRange(department.Select(Line));
        }

        TextFormat.AppendLines(text, lines, TextFormat.MaxListLines * 2);
        text.Append('\n').Append(TextFormat.Link(shoppingUrl, "Открыть список"));
        return text.ToString();
    }

    private static string Line(NeededItem item)
    {
        var quantity = string.IsNullOrWhiteSpace(item.Quantity) ? string.Empty : $" — {TextFormat.Escape(item.Quantity)}";
        return $"• {(item.Urgent ? "❗ " : string.Empty)}{TextFormat.Escape(item.Name)}{quantity}";
    }
}

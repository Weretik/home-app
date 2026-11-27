using Gifts.Contracts;
using Todos.Domain.Categories;
using Todos.Domain.Parties;
using Todos.Domain.Tasks;

namespace Todos.Application.Integrations.Gifts;

/// <summary>How a planned gift looks as a to-do (spec gifts/002, R-004); pure, so it is unit-tested.</summary>
public static class GiftTask
{
    /// <summary>The seeded category «🛍️ Покупки» (Todos CategorySeed); a deleted category leaves the task without one.</summary>
    public static readonly TodoCategoryId ShoppingCategoryId = new(new Guid("0193b6a0-0000-7000-8000-000000000070"));

    private const string Prefix = "🎁 Купити подарунок: ";

    public static string Title(string giftName, GiftParty recipient)
    {
        var suffix = recipient switch
        {
            GiftParty.Maria => " (для Маші)",
            GiftParty.Vitaliy => " (для Віталія)",
            _ => string.Empty,
        };
        var room = TodoTask.MaxTitleLength - Prefix.Length - suffix.Length;
        var name = giftName.Trim();
        if (name.Length > room)
            name = name[..(room - 1)] + "…";
        return Prefix + name + suffix;
    }

    public static string Notes(string link) => $"Подарунок на сайті: {link}";

    public static Assignee AssigneeOf(GiftParty giver) => giver switch
    {
        GiftParty.Maria => Assignee.Maria,
        GiftParty.Vitaliy => Assignee.Vitaliy,
        _ => Assignee.Shared,
    };
}

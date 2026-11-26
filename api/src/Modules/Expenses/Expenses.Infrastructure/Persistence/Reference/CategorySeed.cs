namespace Expenses.Infrastructure.Persistence.Reference;

/// <summary>
/// The categories of the old «Справочник» sheet on the day of the move, with fixed ids so the migration that inserts
/// them is stable. Later changes to categories are made in the app, not here.
/// </summary>
internal static class CategorySeed
{
    public static readonly IReadOnlyList<(Guid Id, string Name, int SortOrder)> Items =
    [
        new(new Guid("86c6ddca-a991-5d9b-97b9-468b90f50c4c"), "Продукты", 10),
        new(new Guid("12b7f2d2-0f0c-5d11-9691-86b3bc919ffe"), "Быт", 20),
        new(new Guid("35252225-cd52-5360-b8a6-c1d16d8fe1e9"), "Личные Виталий", 30),
        new(new Guid("e423ab6d-bafe-5a11-a1b0-fbfc88e4bb08"), "Личные Маша", 40),
        new(new Guid("77ad0660-c002-50f6-ad87-b04c1762cf13"), "Коммуналка", 50),
        new(new Guid("2e1a44f6-fdf2-54ba-998b-ed490c80098d"), "Кафе, доставка, развлечения", 60),
        new(new Guid("361658b1-d605-57e0-b6e6-37f5c8c80e7f"), "Здоровье", 70),
        new(new Guid("a064005b-fd96-5454-b17a-8bf70ff694f2"), "Одежда", 80),
        new(new Guid("eec40aeb-c2a3-5aea-93e8-d4f4929c79aa"), "Ремонт", 90),
        new(new Guid("02d964e6-9360-5e7f-9685-6133d03031ef"), "Подарки", 100),
        new(new Guid("dbd79e28-606e-5efb-bc73-f294ba6d0d5d"), "Уходовые средства и косметика", 110),
        new(new Guid("08305dd6-28c2-59a1-9d98-0026828b4eee"), "Зоотовары", 120),
        new(new Guid("35a6593b-5d73-5fcb-aea8-a2bb9f8e8eb2"), "Мебель и техника", 130),
        new(new Guid("1ab0947d-0601-555e-8bc2-4b4996cbbbe7"), "Сдаваемая квартира", 140),
    ];
}

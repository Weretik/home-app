namespace Gifts.Infrastructure.Persistence.Reference;

/// <summary>
/// The gift categories on the day of the move, with the fixed ids the import SQL refers to. Later changes to
/// categories are made in the app, not here.
/// </summary>
internal static class CategorySeed
{
    public static readonly IReadOnlyList<(Guid Id, string Name, int SortOrder)> Items =
    [
        new(new Guid("a1b9482e-bc3b-53cd-8232-0dbe2b4aa249"), "Техника", 10),
        new(new Guid("f667a762-dff7-5683-8e31-e75127050717"), "Дом и уют", 20),
        new(new Guid("27851329-987a-5bc7-962e-bdea4b943d0c"), "Кухня", 30),
        new(new Guid("dadca160-ff2a-5a99-a27d-c743a38395bb"), "Мебель", 40),
        new(new Guid("40572c1e-f889-5f62-977f-4af1d43581ea"), "Ванная", 50),
        new(new Guid("dcd4bc3e-5b21-5200-a4ac-2fb08317fa2f"), "Одежда и аксессуары", 60),
        new(new Guid("b5691e11-6405-5690-bba9-43e92fef7366"), "Украшения", 70),
        new(new Guid("d5e823ec-44f6-58da-945a-9d4db1e93133"), "Уход и здоровье", 80),
        new(new Guid("9ae4dce6-f172-5f74-a3f6-4979bf0dafbd"), "Спорт", 90),
        new(new Guid("126cde36-f757-558b-9179-e3af435fd0f2"), "Хобби и игры", 100),
        new(new Guid("9dba6a43-1934-58e9-82d6-93a2fe5ca7e6"), "Другое", 110),
    ];
}

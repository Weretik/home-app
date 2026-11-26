namespace Todos.Infrastructure.Persistence.Categories;

/// <summary>
/// The starting categories, with fixed ids so the migration that inserts them is stable and the import script can
/// refer to them. Later changes to categories are made in the app, not here.
/// </summary>
internal static class CategorySeed
{
    public static readonly IReadOnlyList<(Guid Id, string Name, string Emoji, int SortOrder)> Items =
    [
        new(new Guid("0193b6a0-0000-7000-8000-000000000010"), "Дом", "🏠", 10),
        new(new Guid("0193b6a0-0000-7000-8000-000000000020"), "Ремонт", "🔨", 20),
        new(new Guid("0193b6a0-0000-7000-8000-000000000030"), "Здоровье", "🩺", 30),
        new(new Guid("0193b6a0-0000-7000-8000-000000000040"), "Работа", "💼", 40),
        new(new Guid("0193b6a0-0000-7000-8000-000000000050"), "Финансы", "💳", 50),
        new(new Guid("0193b6a0-0000-7000-8000-000000000060"), "Документы", "📄", 60),
        new(new Guid("0193b6a0-0000-7000-8000-000000000070"), "Покупки", "🛍️", 70),
        new(new Guid("0193b6a0-0000-7000-8000-000000000080"), "Отдых", "🌴", 80),
    ];
}

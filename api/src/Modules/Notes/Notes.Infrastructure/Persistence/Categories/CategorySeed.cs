namespace Notes.Infrastructure.Persistence.Categories;

/// <summary>
/// The starting categories, with fixed ids so the migration that inserts them is stable. Later changes to categories
/// are made in the app, not here.
/// </summary>
internal static class CategorySeed
{
    public static readonly IReadOnlyList<(Guid Id, string Name, string Emoji, int SortOrder)> Items =
    [
        new(new Guid("019ac3a0-0000-7000-8000-000000000010"), "Їжа", "🍲", 10),
        new(new Guid("019ac3a0-0000-7000-8000-000000000020"), "Напої", "🥤", 20),
        new(new Guid("019ac3a0-0000-7000-8000-000000000030"), "Квіти", "💐", 30),
        new(new Guid("019ac3a0-0000-7000-8000-000000000040"), "Техніка", "💻", 40),
        new(new Guid("019ac3a0-0000-7000-8000-000000000050"), "Одяг", "👕", 50),
        new(new Guid("019ac3a0-0000-7000-8000-000000000060"), "Дозвілля", "🎬", 60),
        new(new Guid("019ac3a0-0000-7000-8000-000000000070"), "Подорожі", "✈️", 70),
        new(new Guid("019ac3a0-0000-7000-8000-000000000080"), "Здоров'я", "🩺", 80),
        new(new Guid("019ac3a0-0000-7000-8000-000000000090"), "Дім", "🏠", 90),
        new(new Guid("019ac3a0-0000-7000-8000-000000000100"), "Інше", "📝", 100),
    ];
}

namespace BuildingBlocks.Application.Persistence;

/// <summary>
/// Turns free search input into LIKE patterns: one pattern per word, lower-cased, with LIKE wildcards escaped so a
/// user cannot widen the search. A column searched this way gets a trigram (gin_trgm_ops) index.
/// </summary>
public static class SearchPattern
{
    public const string EscapeCharacter = "\\";

    public static IReadOnlyList<string> ContainsEachWord(string? input) =>
        string.IsNullOrWhiteSpace(input)
            ? []
            : input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(word => $"%{Escape(word.ToLowerInvariant())}%")
                .ToArray();

    private static string Escape(string word) =>
        word.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}

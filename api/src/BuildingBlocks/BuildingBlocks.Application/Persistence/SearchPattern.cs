namespace BuildingBlocks.Application.Persistence;

/// <summary>
/// Turns free search input into LIKE patterns: one pattern per word, lower-cased, with LIKE wildcards escaped so a
/// user cannot widen the search. A column searched for words inside it gets a trigram (gin_trgm_ops) index where the
/// database allows one; the prefix patterns serve small tables, where a scan is cheap.
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

    /// <summary>A pattern for values that start with the input: <c>milk%</c>.</summary>
    public static string StartsWith(string input) => $"{Escape(input.ToLowerInvariant())}%";

    /// <summary>
    /// A pattern for values with a later word (after a single space) that starts with the input: <c>% milk%</c>.
    /// Together with <see cref="StartsWith"/> it finds every word start of space-normalised text.
    /// </summary>
    public static string WordStartsWith(string input) => $"% {Escape(input.ToLowerInvariant())}%";

    private static string Escape(string word) =>
        word.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}

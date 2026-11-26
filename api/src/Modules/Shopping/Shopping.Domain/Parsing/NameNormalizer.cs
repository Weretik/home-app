using System.Text.RegularExpressions;

namespace Shopping.Domain.Parsing;

/// <summary>
/// The form in which names and keywords are compared (the bot's <c>norm_</c>): lower case, «ё» as «е», runs of white
/// space as one space, no space at the ends. «Зелёный  Чай » and «зеленый чай» are the same item.
/// </summary>
public static partial class NameNormalizer
{
    public static string Normalize(string? text) =>
        Spaces().Replace((text ?? string.Empty).ToLowerInvariant().Replace('ё', 'е'), " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();
}

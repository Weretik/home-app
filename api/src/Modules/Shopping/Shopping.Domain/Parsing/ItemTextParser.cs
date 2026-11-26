using System.Text.RegularExpressions;

namespace Shopping.Domain.Parsing;

/// <summary>
/// Reads «молоко, хлеб, яйца 10, сыр 300 г» into items, exactly as the Telegram bot's <c>parseItems_</c> did: parts
/// are split on commas, semicolons and new lines; a number with an optional unit at the end (after an optional dash,
/// colon or «x») or at the start is the quantity; the rest is the name, cut to 80 characters and capitalised.
/// Only ASCII digits count, as in the bot's JavaScript.
/// </summary>
public static partial class ItemTextParser
{
    public const int MaxNameLength = 80;
    public const int MaxQuantityLength = 30;

    /// <summary>The bot's <c>UNITS</c>: pieces, weights, volumes and packs, in Russian, as people type them.</summary>
    private const string Units =
        @"шт|штук[аи]?|кило\S*|кг|грам\S*|гр|г|литр\S*|мл|л|уп|упак\S*|пач\S*|бут\S*|бан\S*|пак\S*|десят\S*|ящ\S*|рулон\S*";

    private const string Number = @"[0-9]+(?:[.,][0-9]+)?";

    public static IReadOnlyList<ParsedItem> Parse(string? text) =>
        Parts().Split(text ?? string.Empty)
            .Select(part => Edges().Replace(part, string.Empty))
            .Where(part => part.Length > 0)
            .Select(ParseOne)
            .Where(item => item.Name.Length > 0)
            .ToList();

    private static ParsedItem ParseOne(string part)
    {
        var name = part;
        string? quantity = null;

        var match = QuantityAtEnd().Match(part);
        if (match.Success)
        {
            name = match.Groups[1].Value;
            quantity = Quantity(match.Groups[2].Value, match.Groups[3]);
        }
        else if ((match = QuantityAtStart().Match(part)).Success)
        {
            name = match.Groups[3].Value;
            quantity = Quantity(match.Groups[1].Value, match.Groups[2]);
        }

        name = name.Trim();
        if (name.Length > MaxNameLength)
            name = name[..MaxNameLength];
        if (name.Length > 0)
            name = char.ToUpperInvariant(name[0]) + name[1..];

        return new ParsedItem(name, quantity);
    }

    private static string Quantity(string number, Group unit)
    {
        var quantity = unit.Success ? $"{number} {unit.Value}" : number;
        return quantity.Length > MaxQuantityLength ? quantity[..MaxQuantityLength] : quantity;
    }

    [GeneratedRegex(@"[,;\n]+")]
    private static partial Regex Parts();

    /// <summary>Bullets and dashes in front, white space, dots and exclamation marks at the end.</summary>
    [GeneratedRegex(@"^[\s•*·\-–—]+|[\s.!]+$")]
    private static partial Regex Edges();

    [GeneratedRegex(@"^(.+?)\s*[-–—:]?\s+(?:[xх×]\s*)?(" + Number + @")\s*(" + Units + @")?\.?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex QuantityAtEnd();

    [GeneratedRegex(@"^(" + Number + @")\s*(" + Units + @")?\.?\s+(.+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex QuantityAtStart();
}

using System.Text.RegularExpressions;
using Shopping.Domain.Departments;
using Shopping.Domain.Parsing;

namespace Shopping.Domain.Matching;

/// <summary>
/// Picks the department of an item name, as the bot's <c>deptOf_</c> did. A keyword equal to the whole normalised
/// name scores 1000; otherwise a keyword matches when every one of its words is the start of some word of the name,
/// and scores its length, so «зубн паст» (9) beats «паст» (4). The best score wins; on a tie the rule given first
/// wins (callers pass rules in store order). No match gives the fallback department.
/// </summary>
public sealed partial class DepartmentMatcher
{
    public const int ExactScore = 1000;

    private readonly IReadOnlyList<(string Keyword, string[] Parts, DepartmentId DepartmentId)> _rules;

    public DepartmentMatcher(IEnumerable<KeywordRule> rules, DepartmentId fallbackDepartmentId)
    {
        FallbackDepartmentId = fallbackDepartmentId;
        _rules = rules
            .Select(rule => (Keyword: NameNormalizer.Normalize(rule.Keyword), rule.DepartmentId))
            .Where(rule => rule.Keyword.Length > 0)
            .Select(rule => (rule.Keyword, rule.Keyword.Split(' '), rule.DepartmentId))
            .ToList();
    }

    public DepartmentId FallbackDepartmentId { get; }

    public DepartmentId Match(string name)
    {
        var normalized = NameNormalizer.Normalize(name);
        var words = WordSeparators().Split(normalized).Where(w => w.Length > 0).ToArray();

        DepartmentId? best = null;
        var bestScore = 0;
        foreach (var (keyword, parts, departmentId) in _rules)
        {
            var score = keyword == normalized
                ? ExactScore
                : parts.All(part => words.Any(word => word.StartsWith(part, StringComparison.Ordinal))) ? keyword.Length : 0;
            if (score > bestScore)
            {
                bestScore = score;
                best = departmentId;
            }
        }

        return best ?? FallbackDepartmentId;
    }

    /// <summary>The bot's word split: anything but Latin and Cyrillic (with Ukrainian) letters, apostrophes and digits.</summary>
    [GeneratedRegex("[^a-zа-яіїєґ'0-9]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex WordSeparators();
}

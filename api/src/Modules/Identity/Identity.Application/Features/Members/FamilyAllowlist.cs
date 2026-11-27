namespace Identity.Application.Features.Members;

/// <summary>
/// The e-mail addresses that may sign in (setting <c>Identity:AllowedEmails</c>), compared trimmed and
/// case-insensitively (spec identity/001, R-002). The position says who the member is (spec identity/002, R-001,
/// ADR 0038): entry 0 is Vitaliy, entry 1 is Maria; any later entry may sign in but is nobody in particular.
/// </summary>
public sealed class FamilyAllowlist
{
    private static readonly FamilyPerson[] ByPosition = [FamilyPerson.Vitaliy, FamilyPerson.Maria];

    private readonly Dictionary<string, FamilyPerson?> _emails = new(StringComparer.Ordinal);

    /// <param name="emails">The entries in setting order; an empty entry keeps its position but lets nobody in.</param>
    public FamilyAllowlist(IEnumerable<string?> emails)
    {
        var position = 0;
        foreach (var email in emails)
        {
            var normalized = Normalize(email ?? string.Empty);
            FamilyPerson? person = position < ByPosition.Length ? ByPosition[position] : null;
            if (normalized.Length > 0)
                _emails.TryAdd(normalized, person);
            position++;
        }
    }

    public int Count => _emails.Count;

    public bool Contains(string? email) => email is not null && _emails.ContainsKey(Normalize(email));

    /// <summary>Who the member with this e-mail is, or null for an e-mail past the first two or not on the list.</summary>
    public FamilyPerson? PersonOf(string? email) =>
        email is not null && _emails.TryGetValue(Normalize(email), out var person) ? person : null;

    /// <summary>The form an e-mail is stored and compared in.</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}

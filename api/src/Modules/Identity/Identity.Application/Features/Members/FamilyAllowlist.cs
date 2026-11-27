namespace Identity.Application.Features.Members;

/// <summary>The e-mail addresses that may sign in (setting <c>Identity:AllowedEmails</c>), compared trimmed and case-insensitively (R-002).</summary>
public sealed class FamilyAllowlist(IEnumerable<string> emails)
{
    private readonly HashSet<string> _emails = emails
        .Select(Normalize)
        .Where(email => email.Length > 0)
        .ToHashSet(StringComparer.Ordinal);

    public int Count => _emails.Count;

    public bool Contains(string? email) => email is not null && _emails.Contains(Normalize(email));

    /// <summary>The form an e-mail is stored and compared in.</summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}

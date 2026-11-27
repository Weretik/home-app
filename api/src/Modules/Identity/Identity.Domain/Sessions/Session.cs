using BuildingBlocks.Domain.Modeling;

namespace Identity.Domain.Sessions;

/// <summary>
/// One signed-in device of a family member (ADR 0037). The token itself is never stored, only its hash. A session
/// lives <see cref="Lifetime"/> after its last use; using it slides the expiry, writing at most once per
/// <see cref="TouchInterval"/>. Sign-out revokes it.
/// </summary>
public sealed class Session : Entity<SessionId>, IAggregateRoot
{
    public const int TokenHashLength = 64;
    public const int MaxEmailLength = 320;
    public const int MaxNameLength = 200;
    public const int MaxPictureUrlLength = 2048;

    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);
    public static readonly TimeSpan TouchInterval = TimeSpan.FromHours(1);

    /// <summary>How long an ended session is kept before a sign-in of its member deletes it (R-012).</summary>
    public static readonly TimeSpan KeepEnded = TimeSpan.FromDays(30);

    private Session()
    {
    }

    /// <summary>Lower-case hex SHA-256 of the bearer token.</summary>
    public string TokenHash { get; private init; } = string.Empty;

    public string Email { get; private init; } = string.Empty;

    public string Name { get; private init; } = string.Empty;

    public string? PictureUrl { get; private init; }

    public DateTimeOffset CreatedAtUtc { get; private init; }

    public DateTimeOffset LastSeenAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public MemberProfile Member => new(Email, Name, PictureUrl);

    public static Session Start(string tokenHash, MemberProfile member, DateTimeOffset now) => new()
    {
        Id = SessionId.New(now),
        TokenHash = tokenHash,
        Email = member.Email,
        Name = Cut(member.Name, MaxNameLength) ?? member.Email,
        PictureUrl = member.PictureUrl is { Length: <= MaxPictureUrlLength } url ? url : null,
        CreatedAtUtc = now,
        LastSeenAtUtc = now,
        ExpiresAtUtc = now + Lifetime,
    };

    public bool IsActiveAt(DateTimeOffset now) => RevokedAtUtc is null && now < ExpiresAtUtc;

    /// <summary>Ended (revoked or expired) more than <see cref="KeepEnded"/> before <paramref name="now"/>.</summary>
    public bool IsStaleAt(DateTimeOffset now) => (RevokedAtUtc ?? ExpiresAtUtc) + KeepEnded < now;

    /// <summary>Slides the expiry when the last recorded use is older than <see cref="TouchInterval"/>.</summary>
    /// <returns>Whether anything changed and needs saving.</returns>
    public bool Touch(DateTimeOffset now)
    {
        if (!IsActiveAt(now) || now - LastSeenAtUtc < TouchInterval)
            return false;

        LastSeenAtUtc = now;
        ExpiresAtUtc = now + Lifetime;
        return true;
    }

    /// <summary>Ends the session; a second call keeps the first time.</summary>
    public void Revoke(DateTimeOffset now) => RevokedAtUtc ??= now;

    private static string? Cut(string? value, int max)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}

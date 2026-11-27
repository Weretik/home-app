namespace Identity.Contracts;

/// <summary>
/// Who sends the current request, for modules that record or filter by the signed-in person (ADR 0040). Modules use
/// this instead of reading claims or <c>HttpContext.User</c>; the Identity module knows how a session is carried.
/// </summary>
public interface ICurrentMember
{
    /// <summary>
    /// The signed-in member, or null outside a member's request: the deploy check key, background work, no request.
    /// </summary>
    SignedInMember? Get();
}

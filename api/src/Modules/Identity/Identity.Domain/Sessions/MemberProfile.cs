namespace Identity.Domain.Sessions;

/// <summary>Who a session belongs to, as Google reported it at sign-in: the e-mail is the member's identity.</summary>
public sealed record MemberProfile(string Email, string Name, string? PictureUrl);

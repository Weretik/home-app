namespace Identity.Application.Google;

/// <summary>What a verified Google ID token says about the person.</summary>
public sealed record GoogleIdentity(string Email, bool EmailVerified, string? Name, string? PictureUrl);

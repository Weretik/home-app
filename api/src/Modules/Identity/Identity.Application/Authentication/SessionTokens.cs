using System.Security.Cryptography;
using System.Text;
using Identity.Domain.Sessions;

namespace Identity.Application.Authentication;

/// <summary>The bearer token of a session: 32 random bytes in base64url; only its SHA-256 is stored (R-004).</summary>
public static class SessionTokens
{
    private const int TokenBytes = 32;

    public static string New() => Base64Url(RandomNumberGenerator.GetBytes(TokenBytes));

    /// <summary>Lower-case hex SHA-256, <see cref="Session.TokenHashLength"/> characters.</summary>
    public static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Notifications.Infrastructure.Push;

/// <summary>
/// The VAPID authorization of RFC 8292: an ES256 JWT for the push service's origin, signed with the private key, sent
/// as <c>Authorization: vapid t=&lt;jwt&gt;, k=&lt;public key&gt;</c>.
/// </summary>
internal static class VapidToken
{
    private static readonly string Header = Base64Url.Encode("""{"typ":"JWT","alg":"ES256"}"""u8);

    public static string Authorization(Uri endpoint, string subject, string publicKey, string privateKey, DateTimeOffset now)
    {
        var audience = endpoint.GetLeftPart(UriPartial.Authority);
        var claims = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["aud"] = audience,
            ["exp"] = now.AddHours(12).ToUnixTimeSeconds(),
            ["sub"] = subject,
        });
        var unsigned = $"{Header}.{Base64Url.Encode(Encoding.UTF8.GetBytes(claims))}";

        using var key = ECDsa.Create(PushEncryption.PointParameters(Base64Url.Decode(publicKey), Base64Url.Decode(privateKey)));
        // IEEE P1363 (r ‖ s, 64 bytes) is what JWS ES256 needs; it is .NET's default format.
        var signature = key.SignData(Encoding.ASCII.GetBytes(unsigned), HashAlgorithmName.SHA256);
        return $"vapid t={unsigned}.{Base64Url.Encode(signature)}, k={publicKey.Trim().TrimEnd('=')}";
    }
}

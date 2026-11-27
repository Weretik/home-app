namespace Notifications.Infrastructure.Push;

/// <summary>Base64url without padding (RFC 4648 §5), as Web Push and JWT use it; decoding also accepts padding and plain base64.</summary>
internal static class Base64Url
{
    public static string Encode(ReadOnlySpan<byte> bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static byte[] Decode(string text)
    {
        var base64 = text.Trim().TrimEnd('=').Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '='));
    }
}

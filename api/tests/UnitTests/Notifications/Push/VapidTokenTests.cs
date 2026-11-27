using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Notifications.Infrastructure.Push;

namespace UnitTests.Notifications.Push;

/// <summary>The VAPID authorization of RFC 8292 (spec notifications/002, R-110).</summary>
public sealed class VapidTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 27, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void The_Token_IsAnEs256Jwt_ForThePushServiceOrigin_SignedWithTheKey()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var parameters = key.ExportParameters(true);
        var publicKey = Base64Url.Encode([4, .. parameters.Q.X!, .. parameters.Q.Y!]);
        var privateKey = Base64Url.Encode(parameters.D);

        var header = VapidToken.Authorization(
            new Uri("https://fcm.googleapis.com/fcm/send/abc:def"), "https://home.example", publicKey, privateKey, Now);

        Assert.StartsWith("vapid t=", header, StringComparison.Ordinal);
        Assert.EndsWith($", k={publicKey}", header, StringComparison.Ordinal);
        var jwt = header["vapid t=".Length..header.IndexOf(',', StringComparison.Ordinal)];
        var parts = jwt.Split('.');
        Assert.Equal("""{"typ":"JWT","alg":"ES256"}""", Encoding.UTF8.GetString(Base64Url.Decode(parts[0])));
        using var claims = JsonDocument.Parse(Base64Url.Decode(parts[1]));
        Assert.Equal("https://fcm.googleapis.com", claims.RootElement.GetProperty("aud").GetString());
        Assert.Equal("https://home.example", claims.RootElement.GetProperty("sub").GetString());
        Assert.Equal(Now.AddHours(12).ToUnixTimeSeconds(), claims.RootElement.GetProperty("exp").GetInt64());
        var signature = Base64Url.Decode(parts[2]);
        Assert.Equal(64, signature.Length);
        Assert.True(key.VerifyData(Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}"), signature, HashAlgorithmName.SHA256));
    }
}

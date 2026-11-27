using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Notifications.Infrastructure.Push;

namespace UnitTests.Notifications.Push;

/// <summary>RFC 8291 message encryption (spec notifications/002, SC-107).</summary>
public sealed class PushEncryptionTests
{
    // RFC 8291, Appendix A.
    private const string Plaintext = "When I grow up, I want to be a watermelon";
    private const string ServerPrivate = "yfWPiYE-n46HLnH0KqZOF1fJJU3MYrct3AELtAQ-oRw";
    private const string ServerPublic = "BP4z9KsN6nGRTbVYI_c7VJSPQTBtkgcy27mlmlMoZIIgDll6e3vCYLocInmYWAmS6TlzAC8wEqKK6PBru3jl7A8";
    private const string BrowserPrivate = "q1dXpw3UpT5VOmu_cf_v6ih07Aems3njxI-JWgLcM94";
    private const string BrowserPublic = "BCVxsr7N_eNgVRqvHtD0zTZsEc6-VV-JvLexhqUzORcxaOzi6-AYWXvTBHm4bjyPjs7Vd8pZGH6SRpkNtoIAiw4";
    private const string AuthSecret = "BTBZMqHH6r4Tts7J_aSIgg";
    private const string Salt = "DGv6ra1nlYgDCS1FRnbzlw";
    private const string Expected =
        "DGv6ra1nlYgDCS1FRnbzlwAAEABBBP4z9KsN6nGRTbVYI_c7VJSPQTBtkgcy27mlmlMoZIIgDll6e3vCYLocInmYWAmS6TlzAC8wEqKK6PBru3jl7A_yl95bQpu6cVPTpK4Mqgkf1CXztLVBSt2Ks3oZwbuwXPXLWyouBWLVWGNWQexSgSxsj_Qulcy4a-fN";

    [Fact]
    public void The_Rfc8291_Example_EncryptsToThePublishedMessage()
    {
        using var serverKey = ECDiffieHellman.Create(
            PushEncryption.PointParameters(Base64Url.Decode(ServerPublic), Base64Url.Decode(ServerPrivate)));

        var body = PushEncryption.Encrypt(
            Encoding.UTF8.GetBytes(Plaintext), Base64Url.Decode(BrowserPublic), Base64Url.Decode(AuthSecret), serverKey, Base64Url.Decode(Salt));

        Assert.Equal(Expected, Base64Url.Encode(body));
    }

    [Fact]
    public void A_Fresh_Message_DecryptsWithTheBrowsersKey()
    {
        using var browser = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var browserPublic = PushEncryption.PublicPoint(browser);
        var auth = RandomNumberGenerator.GetBytes(16);
        var plaintext = Encoding.UTF8.GetBytes("""{"title":"Дім","body":"Тест"}""");

        var body = PushEncryption.Encrypt(plaintext, browserPublic, auth);

        Assert.Equal(plaintext, Decrypt(body, browser, browserPublic, auth));
    }

    [Fact]
    public void Two_Messages_UseDifferentSaltsAndKeys()
    {
        using var browser = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var browserPublic = PushEncryption.PublicPoint(browser);
        var auth = RandomNumberGenerator.GetBytes(16);

        var first = PushEncryption.Encrypt([1, 2, 3], browserPublic, auth);
        var second = PushEncryption.Encrypt([1, 2, 3], browserPublic, auth);

        Assert.NotEqual(first[..86], second[..86]);
    }

    /// <summary>The browser's side of RFC 8291, written independently of the code under test.</summary>
    private static byte[] Decrypt(byte[] body, ECDiffieHellman browser, byte[] browserPublic, byte[] auth)
    {
        var salt = body[..16];
        Assert.Equal(4096u, BinaryPrimitives.ReadUInt32BigEndian(body.AsSpan(16, 4)));
        var keyLength = body[20];
        var serverPublic = body[21..(21 + keyLength)];
        var record = body[(21 + keyLength)..];

        using var server = ECDiffieHellman.Create(PushEncryption.PointParameters(serverPublic));
        var secret = browser.DeriveRawSecretAgreement(server.PublicKey);
        var info = Encoding.ASCII.GetBytes("WebPush: info\0").Concat(browserPublic).Concat(serverPublic).ToArray();
        var ikm = HKDF.DeriveKey(HashAlgorithmName.SHA256, secret, 32, auth, info);
        var cek = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 16, salt, Encoding.ASCII.GetBytes("Content-Encoding: aes128gcm\0"));
        var nonce = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 12, salt, Encoding.ASCII.GetBytes("Content-Encoding: nonce\0"));

        var plain = new byte[record.Length - 16];
        using var aes = new AesGcm(cek, 16);
        aes.Decrypt(nonce, record[..^16], record[^16..], plain);
        Assert.Equal(2, plain[^1]);
        return plain[..^1];
    }
}

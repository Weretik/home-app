using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Notifications.Infrastructure.Push;

/// <summary>
/// Message encryption for Web Push, RFC 8291 with the <c>aes128gcm</c> content coding of RFC 8188: an ephemeral P-256
/// key and a random salt per message, ECDH with the browser's key, HKDF-SHA256 with the browser's auth secret, one
/// record with the <c>0x02</c> padding delimiter, AES-128-GCM. Only <c>System.Security.Cryptography</c>.
/// </summary>
internal static class PushEncryption
{
    private const int RecordSize = 4096;
    private static readonly byte[] KeyInfoLabel = Encoding.ASCII.GetBytes("WebPush: info\0");
    private static readonly byte[] CekInfo = Encoding.ASCII.GetBytes("Content-Encoding: aes128gcm\0");
    private static readonly byte[] NonceInfo = Encoding.ASCII.GetBytes("Content-Encoding: nonce\0");

    /// <summary>Encrypts <paramref name="plaintext"/> for a browser with a fresh key and salt.</summary>
    public static byte[] Encrypt(byte[] plaintext, byte[] browserPublicKey, byte[] authSecret)
    {
        using var serverKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        return Encrypt(plaintext, browserPublicKey, authSecret, serverKey, RandomNumberGenerator.GetBytes(16));
    }

    /// <summary>The same with a given server key and salt, for the RFC 8291 example in the tests.</summary>
    public static byte[] Encrypt(byte[] plaintext, byte[] browserPublicKey, byte[] authSecret, ECDiffieHellman serverKey, byte[] salt)
    {
        if (plaintext.Length + 1 + 16 > RecordSize - 86)
            throw new ArgumentException("The push message is too long for one record.", nameof(plaintext));

        var serverPublicKey = PublicPoint(serverKey);
        using var browserKey = ECDiffieHellman.Create(PointParameters(browserPublicKey));
        var sharedSecret = serverKey.DeriveRawSecretAgreement(browserKey.PublicKey);

        var keyInfo = Concat(KeyInfoLabel, browserPublicKey, serverPublicKey);
        var prkKey = HKDF.Extract(HashAlgorithmName.SHA256, sharedSecret, authSecret);
        var ikm = HKDF.Expand(HashAlgorithmName.SHA256, prkKey, 32, keyInfo);
        var prk = HKDF.Extract(HashAlgorithmName.SHA256, ikm, salt);
        var cek = HKDF.Expand(HashAlgorithmName.SHA256, prk, 16, CekInfo);
        var nonce = HKDF.Expand(HashAlgorithmName.SHA256, prk, 12, NonceInfo);

        var padded = new byte[plaintext.Length + 1];
        plaintext.CopyTo(padded, 0);
        padded[^1] = 2; // the last (only) record's delimiter, no further padding
        var cipher = new byte[padded.Length];
        var tag = new byte[16];
        using (var aes = new AesGcm(cek, 16))
            aes.Encrypt(nonce, padded, cipher, tag);

        var header = new byte[16 + 4 + 1];
        salt.CopyTo(header, 0);
        BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(16), RecordSize);
        header[20] = (byte)serverPublicKey.Length;
        return Concat(header, serverPublicKey, cipher, tag);
    }

    /// <summary>The uncompressed point 0x04 ‖ X ‖ Y of a P-256 key.</summary>
    public static byte[] PublicPoint(ECDiffieHellman key)
    {
        var q = key.ExportParameters(false).Q;
        return Concat([4], q.X!, q.Y!);
    }

    /// <summary>Parameters of a P-256 public key from its uncompressed point; <paramref name="privateKey"/> adds D.</summary>
    public static ECParameters PointParameters(byte[] point, byte[]? privateKey = null)
    {
        if (point.Length != 65 || point[0] != 4)
            throw new ArgumentException("Expected an uncompressed P-256 point of 65 bytes.", nameof(point));
        return new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = point[1..33], Y = point[33..65] },
            D = privateKey,
        };
    }

    private static byte[] Concat(params byte[][] parts)
    {
        var result = new byte[parts.Sum(p => p.Length)];
        var offset = 0;
        foreach (var part in parts)
        {
            part.CopyTo(result, offset);
            offset += part.Length;
        }

        return result;
    }
}

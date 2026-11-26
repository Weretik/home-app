using System.Security.Cryptography;
using System.Text;

namespace Shopping.Infrastructure.Persistence.Reference;

/// <summary>
/// Name-based UUIDs (RFC 9562 version 5) for the seeded rows: the same name always gives the same id, so the
/// migration's seed data is stable and an import script can compute the ids too
/// (<c>uuid_generate_v5('6c1e0b7a-2f4d-4b8e-9a3c-5d7f1e2b4c60', 'department:Хлеб')</c>).
/// </summary>
internal static class SeedIds
{
    public static readonly Guid Namespace = new("6c1e0b7a-2f4d-4b8e-9a3c-5d7f1e2b4c60");

    public static Guid Department(string name) => Version5($"department:{name}");

    public static Guid Keyword(string keyword) => Version5($"keyword:{keyword}");

    public static Guid Version5(string name)
    {
        var bytes = new byte[16 + Encoding.UTF8.GetByteCount(name)];
        Namespace.TryWriteBytes(bytes, bigEndian: true, out _);
        Encoding.UTF8.GetBytes(name, bytes.AsSpan(16));
#pragma warning disable CA5350 // SHA-1 is what version 5 UUIDs are defined with; nothing secret depends on it.
        var hash = SHA1.HashData(bytes);
#pragma warning restore CA5350
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
        return new Guid(hash.AsSpan(0, 16), bigEndian: true);
    }
}

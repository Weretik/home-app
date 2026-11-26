using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Shopping.Application.Idempotency;

/// <summary>
/// The repeat rules of ADR 0007 for the module's creates: the same key with the same data returns the first outcome,
/// the same key with other data is a conflict.
/// </summary>
public static class IdempotentCreates
{
    public static readonly Error KeyReused = Error.Conflict(
        "shopping.idempotency_key_reused", "This Idempotency-Key was already used for different data.");

    public static string Fingerprint<T>(T payload) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload))));

    public static CreateRequest Record<T>(string key, string kind, string fingerprint, T outcome, DateTimeOffset now) =>
        new(key, kind, fingerprint, JsonSerializer.Serialize(outcome), now);

    /// <summary>The earlier outcome for this key, a conflict when the data differs, or null when the key is new.</summary>
    public static async Task<Result<T>?> EarlierOutcomeAsync<T>(
        IShoppingDbContext db, string key, string kind, string fingerprint, CancellationToken cancellationToken)
    {
        var earlier = await db.CreateRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key, cancellationToken);
        if (earlier is null)
            return null;

        return earlier.Kind == kind && earlier.Fingerprint == fingerprint
            ? JsonSerializer.Deserialize<T>(earlier.Outcome)!
            : KeyReused;
    }
}

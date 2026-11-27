using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Notes.Application.Idempotency;

/// <summary>
/// The repeat rules of ADR 0007 for the module's creates: the same key with the same data returns the first result,
/// the same key with other data is a conflict.
/// </summary>
public static class IdempotentCreates
{
    public static readonly Error KeyReused = Error.Conflict(
        "notes.idempotency_key_reused", "Цей Idempotency-Key уже використано для інших даних.");

    public static string Fingerprint<T>(T payload) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload))));

    /// <summary>The earlier result for this key, a conflict when the data differs, or null when the key is new.</summary>
    public static async Task<Result<NoteCreated>?> EarlierResultAsync(
        INotesDbContext db, string key, string kind, string fingerprint, CancellationToken cancellationToken)
    {
        var earlier = await db.CreateRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key, cancellationToken);
        if (earlier is null)
            return null;

        return earlier.Kind == kind && earlier.Fingerprint == fingerprint
            ? new NoteCreated(earlier.EntityId)
            : KeyReused;
    }
}

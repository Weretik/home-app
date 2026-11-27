using System.Text.Json;
using System.Text.Json.Serialization;
using Expenses.Domain.Budget.Amounts;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Expenses.Infrastructure.Persistence.Reference;

/// <summary>
/// An <see cref="AmountHistory"/> as a <c>jsonb</c> column of its row: <c>[{"from":"2000-01-01","amount":7000.00}]</c>,
/// oldest first. The steps are always read with the row and never queried by SQL (ADR 0041). The stored shape is its
/// own record with fixed property names, so renaming the domain type changes neither stored rows nor the HTTP contract.
/// </summary>
internal static class AmountHistoryColumn
{
    public const string Type = "jsonb";

    private static readonly JsonSerializerOptions Json = new();

    public static readonly ValueConverter<AmountHistory, string> Converter = new(
        history => Write(history),
        json => Read(json));

    public static readonly ValueComparer<AmountHistory> Comparer = new(
        (a, b) => a == null ? b == null : a.Equals(b),
        history => history.GetHashCode(),
        history => history);

    private static string Write(AmountHistory history) =>
        JsonSerializer.Serialize(history.Steps.Select(s => new StoredStep(s.From, s.Amount)), Json);

    /// <summary>
    /// Reads the steps. An empty list is the column default kept while the previous release may still insert a row
    /// without the column (SDD 006 data model, until the follow-up migration fills it from the old amount): it reads as
    /// one step of 0 from the beginning, which every rule treats as "no limit", instead of failing the request.
    /// </summary>
    private static AmountHistory Read(string json)
    {
        var steps = JsonSerializer.Deserialize<List<StoredStep>>(json, Json) ?? [];
        return steps.Count == 0
            ? AmountHistory.Start(0)
            : AmountHistory.From(steps.Select(s => new AmountStep(s.From, s.Amount)));
    }

    private sealed record StoredStep(
        [property: JsonPropertyName("from"), JsonRequired] DateOnly From,
        [property: JsonPropertyName("amount"), JsonRequired] decimal Amount);
}

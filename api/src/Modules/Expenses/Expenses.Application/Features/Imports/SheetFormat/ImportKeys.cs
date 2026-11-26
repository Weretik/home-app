using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Expenses.Application.Features.Imports.SheetFormat;

/// <summary>
/// Gives every imported row a stable key: a hash of its values plus how many identical rows came before it in the
/// same tab (the sheet has real duplicates, such as two equal water bills on one day). Importing the same sheet again
/// produces the same keys, so the rows already stored are skipped; a row added to the sheet later gets a new key.
/// </summary>
internal static class ImportKeys
{
    public static IReadOnlyList<string> For<T>(string kind, IEnumerable<T> rows, Func<T, string> canonical)
    {
        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        var keys = new List<string>();
        foreach (var row in rows)
        {
            var hash = Hash(canonical(row));
            var occurrence = seen.TryGetValue(hash, out var count) ? count + 1 : 1;
            seen[hash] = occurrence;
            keys.Add($"sheet:{kind}:{hash}:{occurrence.ToString(CultureInfo.InvariantCulture)}");
        }

        return keys;
    }

    public static string Expense(SheetExpense e) =>
        Join(e.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), e.Payer.ToString(), e.CategoryName.ToUpperInvariant(),
            Money(e.Amount), e.PaymentMethod.ToString(), e.Comment?.Trim() ?? string.Empty);

    public static string Income(SheetIncome i) =>
        Join(i.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), i.Person.ToString(), Money(i.Amount),
            i.Comment?.Trim() ?? string.Empty);

    private static string Money(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Join(params string[] values) => string.Join('\u001f', values);

    private static string Hash(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..32];
}

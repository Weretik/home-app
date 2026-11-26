using System.Globalization;
using System.Text.RegularExpressions;
using Expenses.Domain.Entries;
using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Imports.SheetFormat;

/// <summary>
/// Reads single cells the way the old Google Sheet shows them: dates as <c>01.10.2026</c>, amounts as
/// <c>2,424.00</c> (comma for thousands, point for kopecks), people and payment methods as Russian words.
/// </summary>
internal static partial class SheetValues
{
    private static readonly string[] DateFormats = ["dd.MM.yyyy", "d.M.yyyy"];

    private static readonly Dictionary<string, Payer> Payers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Виталий"] = Payer.Vitaliy,
        ["Мария"] = Payer.Maria,
        ["Маша"] = Payer.Maria,
        ["Общее"] = Payer.Shared,
    };

    private static readonly Dictionary<string, Person> People = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Виталий"] = Person.Vitaliy,
        ["Мария"] = Person.Maria,
        ["Маша"] = Person.Maria,
    };

    private static readonly Dictionary<string, PaymentMethod> PaymentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Карта"] = PaymentMethod.Card,
        ["Наличные"] = PaymentMethod.Cash,
    };

    public static DateOnly? ParseDate(string cell) =>
        DateOnly.TryParseExact(cell.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    /// <summary>
    /// Only the sheet's own format is accepted (<c>2,424.00</c> or <c>170</c>), so <c>1 000,50</c> from another
    /// locale is rejected instead of being read as a hundred thousand.
    /// </summary>
    public static decimal? ParseAmount(string cell)
    {
        var text = cell.Trim();
        if (!AmountFormat().IsMatch(text))
            return null;

        return decimal.Parse(text.Replace(",", "", StringComparison.Ordinal), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }

    /// <summary>An amount with an optional sign in front: <c>-111.10</c>, <c>−111.10</c> or <c>+500</c>.</summary>
    public static decimal? ParseSignedAmount(string cell)
    {
        var text = cell.Trim();
        var negative = text.StartsWith('-') || text.StartsWith('\u2212');
        if (negative || text.StartsWith('+'))
            text = text[1..].TrimStart();

        return ParseAmount(text) is { } amount ? (negative ? -amount : amount) : null;
    }

    public static Payer? ParsePayer(string cell) => Payers.TryGetValue(cell.Trim(), out var payer) ? payer : null;

    public static Person? ParsePerson(string cell) => People.TryGetValue(cell.Trim(), out var person) ? person : null;

    public static PaymentMethod? ParsePaymentMethod(string cell) =>
        PaymentMethods.TryGetValue(cell.Trim(), out var method) ? method : null;

    [GeneratedRegex(@"^(\d{1,3}(,\d{3})+|\d+)(\.\d{1,2})?$", RegexOptions.CultureInvariant)]
    private static partial Regex AmountFormat();
}

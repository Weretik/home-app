using System.Globalization;
using System.Net;
using System.Text;

namespace Notifications.Application.Digests.Texts;

/// <summary>The pieces every message is built from: HTML escaping, hryvnias, dates, names and cut lists.</summary>
public static class TextFormat
{
    /// <summary>Telegram refuses messages over 4096 characters; lists are cut well before that.</summary>
    public const int MaxListLines = 25;

    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    private static readonly string[] Months =
    [
        "січень", "лютий", "березень", "квітень", "травень", "червень", "липень", "серпень", "вересень", "жовтень", "листопад", "грудень",
    ];

    public static string Escape(string text) => WebUtility.HtmlEncode(text);

    /// <summary>12 345 ₴ (no kopecks when there are none).</summary>
    public static string Money(decimal amount)
    {
        var format = decimal.Round(amount) == amount ? "#,0" : "#,0.00";
        return amount.ToString(format, Ukrainian).Replace(' ', ' ') + " ₴";
    }

    /// <summary>+1 234 ₴ or −1 234 ₴.</summary>
    public static string Signed(decimal amount) => (amount < 0 ? "−" : "+") + Money(Math.Abs(amount));

    public static string Day(DateOnly day) => day.ToString("dd.MM", CultureInfo.InvariantCulture);

    public static string MonthName(int month) => Months[month - 1];

    /// <summary>Ukrainian names of the people and of the shared budget, from the codes the modules use.</summary>
    public static string Person(string code) => code switch
    {
        "Vitaliy" => "Віталій",
        "Maria" => "Маша",
        "Shared" or "Family" => "Спільне",
        "Together" => "Разом",
        _ => code,
    };

    /// <summary>A person's name for addressing them (the vocative: «Віталію», «Машо»); other codes as <see cref="Person"/>.</summary>
    public static string Address(string code) => code switch
    {
        "Vitaliy" => "Віталію",
        "Maria" => "Машо",
        _ => Person(code),
    };

    public static string Link(string url, string text) => $"<a href=\"{Escape(url)}\">{Escape(text)}</a>";

    /// <summary>Appends up to <paramref name="max"/> lines and «…і ще N» for the rest.</summary>
    public static void AppendLines(StringBuilder text, IReadOnlyList<string> lines, int max = MaxListLines)
    {
        foreach (var line in lines.Take(max))
            text.Append(line).Append('\n');
        if (lines.Count > max)
            text.Append("…і ще ").Append(lines.Count - max).Append('\n');
    }

    /// <summary>A title shortened for a button.</summary>
    public static string Short(string text, int max)
    {
        var trimmed = text.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..(max - 1)] + "…";
    }
}

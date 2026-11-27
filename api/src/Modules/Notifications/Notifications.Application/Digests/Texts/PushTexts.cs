using System.Net;
using System.Text.RegularExpressions;
using Notifications.Application.Digests.Planning;
using Notifications.Application.Push;
using Notifications.Domain.Deliveries;

namespace Notifications.Application.Digests.Texts;

/// <summary>
/// Turns a planned Telegram message into a push (spec notifications/002, R-107): a short title per kind, the text
/// without HTML and without its links (a tap opens the page instead), cut to <see cref="MaxBodyLength"/>.
/// </summary>
public static partial class PushTexts
{
    /// <summary>Keeps the encrypted payload well under the push services' 4 KB.</summary>
    public const int MaxBodyLength = 1000;

    public static PushPayload For(PlannedMessage message) => new(
        Title(message.Kind),
        Plain(message.Text),
        message.Page,
        message.Kind == DigestKind.GiftReminder ? $"gift:{message.Page}" : message.Kind.ToString());

    public static string Title(DigestKind kind) => kind switch
    {
        DigestKind.TodosMorning => "Справи на сьогодні",
        DigestKind.TodosCommon => "Спільні справи",
        DigestKind.GiftReminder => "Час купити подарунок",
        DigestKind.ShoppingEvening => "Список покупок",
        DigestKind.ExpensesWeek => "Витрати за тиждень",
        _ => "Підсумок місяця",
    };

    /// <summary>Telegram HTML as plain text: links dropped, tags removed, entities decoded, empty lines squeezed, cut.</summary>
    public static string Plain(string html)
    {
        var text = Link().Replace(html, string.Empty);
        text = WebUtility.HtmlDecode(Tag().Replace(text, string.Empty));
        var lines = text.Split('\n').Select(line => line.TrimEnd()).ToList();
        text = string.Join('\n', lines).Trim();
        text = BlankLines().Replace(text, "\n\n");
        return text.Length <= MaxBodyLength ? text : text[..(MaxBodyLength - 1)].TrimEnd() + "…";
    }

    [GeneratedRegex(@"<a\b[^>]*>.*?</a>", RegexOptions.Singleline)]
    private static partial Regex Link();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Tag();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex BlankLines();
}

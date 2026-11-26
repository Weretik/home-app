using System.Globalization;

namespace Todos.Api;

/// <summary>
/// Edits send the version they started from in <c>If-Match</c> (the <c>version</c> field of the task or category, quoted
/// or not). A missing or unreadable header becomes version 0, which the command validator rejects.
/// </summary>
internal static class TodoVersionHeader
{
    public const string Name = "If-Match";

    public static uint Parse(string? header)
    {
        var text = header?.Trim() ?? string.Empty;
        if (text.StartsWith("W/", StringComparison.Ordinal))
            text = text[2..];

        return uint.TryParse(text.Trim('"'), NumberStyles.None, CultureInfo.InvariantCulture, out var version)
            ? version
            : 0;
    }
}

using System.Text.Json;
using Notifications.Application.Features.Telegram.Commands.HandleTelegramUpdate;

namespace Notifications.Api.Endpoints.Telegram;

/// <summary>
/// Reads the parts of a Telegram <c>Update</c> the bot uses: a message's text, a pressed button, or the bot's new
/// membership in a chat. Anything else gives null and is ignored.
/// </summary>
internal static class TelegramUpdateParser
{
    public static HandleTelegramUpdateCommand? Parse(JsonElement update)
    {
        if (update.ValueKind != JsonValueKind.Object)
            return null;

        if (update.TryGetProperty("message", out var message) && Chat(message) is { } fromMessage)
            return Command(fromMessage, text: Text(message, "text"));

        if (update.TryGetProperty("callback_query", out var callback)
            && callback.TryGetProperty("message", out var source) && Chat(source) is { } fromButton)
            return Command(fromButton, callbackId: Text(callback, "id"), callbackData: Text(callback, "data"));

        if (update.TryGetProperty("my_chat_member", out var member) && Chat(member) is { } fromMember
            && member.TryGetProperty("new_chat_member", out var status))
            return Command(fromMember, memberStatus: Text(status, "status"));

        return null;
    }

    private static (long Id, bool IsGroup, string? Title)? Chat(JsonElement parent)
    {
        if (!parent.TryGetProperty("chat", out var chat) || !chat.TryGetProperty("id", out var id) || !id.TryGetInt64(out var chatId))
            return null;
        var type = Text(chat, "type");
        return (chatId, type is "group" or "supergroup", Text(chat, "title") ?? Text(chat, "first_name"));
    }

    private static HandleTelegramUpdateCommand Command(
        (long Id, bool IsGroup, string? Title) chat,
        string? text = null,
        string? callbackId = null,
        string? callbackData = null,
        string? memberStatus = null) =>
        new(chat.Id, chat.IsGroup, chat.Title, text, callbackId, callbackData, memberStatus);

    private static string? Text(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}

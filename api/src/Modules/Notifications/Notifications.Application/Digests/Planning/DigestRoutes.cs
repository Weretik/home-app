using Gifts.Contracts;
using Notifications.Domain.Chats;
using Notifications.Domain.Deliveries;

namespace Notifications.Application.Digests.Planning;

/// <summary>
/// Which linked chats get which message (spec notifications/001, R-014): personal messages only to the person's
/// chat; common ones to the family group, or to every linked private chat when there is no group.
/// </summary>
public static class DigestRoutes
{
    private static readonly Audience[] People = [Audience.Vitaliy, Audience.Maria];

    public static IReadOnlyList<Audience> For(DigestKind kind, IReadOnlyDictionary<Audience, long> chats) =>
        kind == DigestKind.TodosMorning ? [.. People.Where(chats.ContainsKey)] : Common(chats);

    public static IReadOnlyList<Audience> Common(IReadOnlyDictionary<Audience, long> chats) =>
        chats.ContainsKey(Audience.Family) ? [Audience.Family] : [.. People.Where(chats.ContainsKey)];

    /// <summary>The giver's chat (both people for «Вместе»); the family group for a giver without a chat of their own.</summary>
    public static IReadOnlyList<Audience> Givers(GiftParty giver, IReadOnlyDictionary<Audience, long> chats)
    {
        Audience[] people = giver switch
        {
            GiftParty.Vitaliy => [Audience.Vitaliy],
            GiftParty.Maria => [Audience.Maria],
            _ => People,
        };
        return [.. people.Select(p => chats.ContainsKey(p) ? p : Audience.Family).Where(chats.ContainsKey).Distinct()];
    }
}

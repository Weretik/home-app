using Identity.Contracts;
using Notifications.Domain.Chats;

namespace Notifications.Application.Push;

/// <summary>The signed-in person as a push audience (ADR 0040), or null for a member without a person (R-101).</summary>
public static class PushPeople
{
    public static Audience? Of(ICurrentMember member) => member.Get()?.Person switch
    {
        HouseholdPerson.Vitaliy => Audience.Vitaliy,
        HouseholdPerson.Maria => Audience.Maria,
        _ => null,
    };
}

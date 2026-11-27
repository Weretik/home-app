using Identity.Contracts;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes;

/// <summary>
/// Who writes a note: the person of the signed-in member from Identity (ADR 0040), mapped to the module's own
/// <see cref="Person"/>; null for a member with no person in the household (R-008).
/// </summary>
public static class NoteAuthors
{
    public static Person? Of(ICurrentMember member) => member.Get()?.Person switch
    {
        HouseholdPerson.Vitaliy => Person.Vitaliy,
        HouseholdPerson.Maria => Person.Maria,
        _ => null,
    };
}

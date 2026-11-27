using Identity.Contracts;
using Notes.Application.Features.Notes;
using Notes.Domain.Notes;

namespace UnitTests.Notes.Input;

/// <summary>Spec notes/001, R-008: the author is the signed-in member's person, never a field of the request.</summary>
public sealed class NoteAuthorsTests
{
    private sealed class Member(SignedInMember? member) : ICurrentMember
    {
        public SignedInMember? Get() => member;
    }

    [Theory]
    [InlineData(HouseholdPerson.Vitaliy, NotePerson.Vitaliy)]
    [InlineData(HouseholdPerson.Maria, NotePerson.Maria)]
    public void The_Household_Person_BecomesTheNotesPerson(HouseholdPerson household, NotePerson person)
    {
        Assert.Equal(person, NoteAuthors.Of(new Member(new SignedInMember("a@b.c", "Ім'я", household))));
    }

    [Fact]
    public void A_Member_WithoutAPerson_OrNoMember_WritesWithoutAnAuthor()
    {
        Assert.Null(NoteAuthors.Of(new Member(new SignedInMember("guest@b.c", "Гість", null))));
        Assert.Null(NoteAuthors.Of(new Member(null)));
    }
}

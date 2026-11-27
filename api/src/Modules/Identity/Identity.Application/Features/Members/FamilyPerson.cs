namespace Identity.Application.Features.Members;

/// <summary>
/// Who a member is in the home, so each page can start on the signed-in person (spec identity/002, R-001). The values
/// match the people of the other modules (Shopping <c>Person</c>, Todos <c>Assignee</c>, Gifts <c>Recipient</c>).
/// </summary>
public enum FamilyPerson
{
    Vitaliy,
    Maria,
}

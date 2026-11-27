namespace Identity.Contracts;

/// <summary>
/// One of the two people of the home, as other modules learn it from <see cref="ICurrentMember"/>. Each module maps it
/// to its own person type (ADR 0040).
/// </summary>
public enum HouseholdPerson
{
    Vitaliy,
    Maria,
}

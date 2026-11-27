namespace Identity.Contracts;

/// <summary>
/// A family member signed in for the current request: e-mail, name and who they are in the home; the person is null
/// for an allowlisted e-mail past the first two (ADR 0038, 0040).
/// </summary>
public sealed record SignedInMember(string Email, string Name, HouseholdPerson? Person);

namespace Expenses.Domain.Parties;

/// <summary>
/// Who an expense is booked to (the sheet's «Кто» column): one of the two people, or the household as a whole for
/// shared spending such as groceries.
/// </summary>
public enum Payer
{
    Vitaliy,
    Maria,
    Shared,
}

namespace Shopping.Contracts;

/// <summary>An item still to buy, with its department for grouping.</summary>
public sealed record NeededItem(string Name, string? Quantity, bool Urgent, string Department, string DepartmentIcon);

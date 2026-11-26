namespace Shopping.Domain.Parsing;

/// <summary>One item read from a typed line: its name (capitalised) and the quantity as typed, or null.</summary>
public sealed record ParsedItem(string Name, string? Quantity);

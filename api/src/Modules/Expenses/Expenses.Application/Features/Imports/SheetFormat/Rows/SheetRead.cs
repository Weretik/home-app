namespace Expenses.Application.Features.Imports.SheetFormat.Rows;

/// <summary>The rows of one tab that could be read, and a message for each cell that could not.</summary>
internal sealed record SheetRead<T>(IReadOnlyList<T> Items, string Field, IReadOnlyList<string> Problems);

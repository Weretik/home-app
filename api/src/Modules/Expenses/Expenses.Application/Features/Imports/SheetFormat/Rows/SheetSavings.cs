using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Imports.SheetFormat.Rows;

/// <summary>A «Копилка» row after reading: a signed amount (plus put in, minus taken out) and its sheet row number.</summary>
internal sealed record SheetSavings(int Row, DateOnly Date, Person Person, decimal Amount, string? Comment);

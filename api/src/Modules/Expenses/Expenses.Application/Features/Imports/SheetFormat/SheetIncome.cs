using Expenses.Domain.Parties;

namespace Expenses.Application.Features.Imports.SheetFormat;

/// <summary>A «Доходы» row after reading, with its sheet row number.</summary>
internal sealed record SheetIncome(int Row, DateOnly Date, Person Person, decimal Amount, string? Comment);

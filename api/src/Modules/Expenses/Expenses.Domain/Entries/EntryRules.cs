using BuildingBlocks.Domain.Results;

namespace Expenses.Domain.Entries;

/// <summary>
/// The checks shared by expenses and incomes. Amounts are hryvnias with kopecks (two digits after the point), dates
/// are calendar days without a time, and a comment is free text.
/// </summary>
public static class EntryRules
{
    public const decimal MaxAmount = 10_000_000m;
    public const int MaxCommentLength = 500;
    public const int MaxImportKeyLength = 100;

    private static readonly DateOnly MinDate = new(2000, 1, 1);
    private static readonly DateOnly MaxDate = new(2100, 12, 31);

    public static Result Check(DateOnly date, decimal amount, string? comment, string? importKey) =>
        Result.Combine(
            Result.Ensure(date >= MinDate && date <= MaxDate, EntryErrors.DateOutOfRange),
            Result.Ensure(amount > 0, EntryErrors.AmountNotPositive),
            Result.Ensure(amount <= MaxAmount, EntryErrors.AmountTooLarge),
            Result.Ensure(decimal.Round(amount, 2) == amount, EntryErrors.AmountTooPrecise),
            Result.Ensure(Normalize(comment) is not { Length: > MaxCommentLength }, EntryErrors.CommentTooLong),
            Result.Ensure(importKey is not { Length: > MaxImportKeyLength }, EntryErrors.ImportKeyTooLong));

    /// <summary>
    /// The same checks for a signed amount, such as a savings movement: plus puts money in, minus takes it out, and
    /// zero means nothing happened.
    /// </summary>
    public static Result CheckSigned(DateOnly date, decimal amount, string? comment, string? importKey) =>
        amount == 0
            ? EntryErrors.AmountZero
            : Check(date, Math.Abs(amount), comment, importKey);

    /// <summary>A comment is trimmed; an empty one is stored as no comment.</summary>
    public static string? Normalize(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
}

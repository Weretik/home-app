using Expenses.Application.Features.Expenses.Queries.ListExpenses;
using Expenses.Domain.Parties;

namespace UnitTests.Expenses.Input;

public sealed class ListExpensesValidatorTests
{
    private static readonly ListExpensesValidator Validator = new();

    [Fact]
    public void A_Month_IsValid()
    {
        Assert.True(Validator.Validate(Query(year: 2026, month: 11)).IsValid);
    }

    [Theory]
    [InlineData("2026-11-01", "2026-11-30")]
    [InlineData("2026-11-26", "2026-11-26")]
    [InlineData("2026-01-01", "2026-12-31")]
    [InlineData("2028-01-01", "2028-12-31")]
    [InlineData("2026-01-01", "2027-01-01")]
    public void A_Period_OfOneTo366Days_IsValid(string from, string to)
    {
        Assert.True(Validator.Validate(Query(from: DateOnly.Parse(from), to: DateOnly.Parse(to))).IsValid);
    }

    [Theory]
    [InlineData("2026-01-01", "2027-01-02")]
    [InlineData("2026-01-01", "2030-01-01")]
    public void A_Period_LongerThan366Days_IsInvalid(string from, string to)
    {
        var result = Validator.Validate(Query(from: DateOnly.Parse(from), to: DateOnly.Parse(to)));

        Assert.Contains(result.Errors, e => e.PropertyName == "To");
    }

    [Fact]
    public void A_Period_ThatEndsBeforeItStarts_IsInvalid()
    {
        var result = Validator.Validate(Query(from: new DateOnly(2026, 11, 26), to: new DateOnly(2026, 11, 25)));

        Assert.Contains(result.Errors, e => e.PropertyName == "To");
    }

    [Fact]
    public void A_Period_OutsideTheSupportedYears_IsInvalid()
    {
        var result = Validator.Validate(Query(from: new DateOnly(1999, 12, 31), to: new DateOnly(2000, 1, 1)));

        Assert.Contains(result.Errors, e => e.PropertyName == "From");
    }

    [Fact]
    public void Both_AMonth_AndAPeriod_OrNeither_OrHalfOfOne_IsInvalid()
    {
        var from = new DateOnly(2026, 11, 1);
        var to = new DateOnly(2026, 11, 30);

        Assert.False(Validator.Validate(Query(year: 2026, month: 11, from: from, to: to)).IsValid);
        Assert.False(Validator.Validate(Query()).IsValid);
        Assert.False(Validator.Validate(Query(year: 2026)).IsValid);
        Assert.False(Validator.Validate(Query(from: from)).IsValid);
        Assert.False(Validator.Validate(Query(year: 2026, to: to)).IsValid);
    }

    [Theory]
    [InlineData(1999, 1)]
    [InlineData(2026, 0)]
    [InlineData(2026, 13)]
    public void A_Month_OutsideTheRange_IsInvalid(int year, int month)
    {
        Assert.False(Validator.Validate(Query(year: year, month: month)).IsValid);
    }

    [Fact]
    public void The_Period_OfAMonth_EndsOnTheFirstOfTheNext()
    {
        Assert.Equal((new DateOnly(2026, 12, 1), new DateOnly(2027, 1, 1)), Query(year: 2026, month: 12).Period());
    }

    [Fact]
    public void The_Period_OfDays_IncludesTheLastDay()
    {
        var query = Query(from: new DateOnly(2026, 11, 1), to: new DateOnly(2026, 11, 26));

        Assert.Equal((new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 27)), query.Period());
    }

    private static ListExpensesQuery Query(int? year = null, int? month = null, DateOnly? from = null, DateOnly? to = null) =>
        new(year, month, from, to, null, (Payer?)null, 1, 50);
}

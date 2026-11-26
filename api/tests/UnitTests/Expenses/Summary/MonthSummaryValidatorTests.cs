using Expenses.Application.Features.Summary.Queries.GetMonthSummary;

namespace UnitTests.Expenses.Summary;

public sealed class MonthSummaryValidatorTests
{
    [Theory]
    [InlineData(2026, 11, true)]
    [InlineData(2026, 0, false)]
    [InlineData(2026, 13, false)]
    [InlineData(1999, 5, false)]
    [InlineData(2101, 5, false)]
    public void A_Month_IsACalendarMonth_In2000To2100(int year, int month, bool valid) =>
        Assert.Equal(valid, new GetMonthSummaryValidator().Validate(new GetMonthSummaryQuery(year, month)).IsValid);
}

using System.Text.Json;
using Expenses.Domain.Budget.Amounts;
using Expenses.Infrastructure.Persistence.Reference;

namespace UnitTests.Expenses.Amounts;

/// <summary>The <c>jsonb</c> shape of a limit's amounts (ADR 0041, SDD 006 data model, audit 2026-11-27 AF-004).</summary>
public sealed class AmountHistoryColumnTests
{
    private static readonly DateOnly December = new(2026, 12, 1);

    [Fact]
    public void A_History_IsWritten_WithFixedNames_OldestFirst()
    {
        var history = AmountHistory.Start(7_000.00m).ChangeFrom(December, 5_000.50m);

        var json = (string)AmountHistoryColumn.Converter.ConvertToProvider(history)!;

        Assert.Equal("""[{"from":"2000-01-01","amount":7000.00},{"from":"2026-12-01","amount":5000.50}]""", json);
    }

    [Fact]
    public void A_Stored_Column_IsRead_BackToTheSameHistory()
    {
        const string json = """[{"from":"2026-12-01","amount":5000},{"from":"2000-01-01","amount":7000.00}]""";

        var history = (AmountHistory)AmountHistoryColumn.Converter.ConvertFromProvider(json)!;

        Assert.Equal([new AmountStep(AmountHistory.Beginning, 7_000m), new AmountStep(December, 5_000m)], history.Steps);
        Assert.Equal(history, AmountHistoryColumn.Converter.ConvertFromProvider(AmountHistoryColumn.Converter.ConvertToProvider(history)));
    }

    [Fact]
    public void An_Empty_Column_ReadsAs_NoLimit_InsteadOfFailing()
    {
        // The default the previous release leaves on a row it inserts (SDD 006 data model).
        var history = (AmountHistory)AmountHistoryColumn.Converter.ConvertFromProvider("[]")!;

        Assert.Equal(AmountHistory.Start(0m), history);
    }

    [Fact]
    public void A_Step_WithoutItsNames_IsRefused_InsteadOfReadAsZero()
    {
        Assert.Throws<JsonException>(() => AmountHistoryColumn.Converter.ConvertFromProvider("""[{"From":"2000-01-01","Value":7000}]"""));
    }

    [Fact]
    public void The_Comparer_SeesAChangedHistory()
    {
        var history = AmountHistory.Start(7_000m);

        Assert.True(AmountHistoryColumn.Comparer.Equals(history, AmountHistory.Start(7_000m)));
        Assert.False(AmountHistoryColumn.Comparer.Equals(history, history.ChangeFrom(December, 5_000m)));
    }
}

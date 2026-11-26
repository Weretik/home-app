using Gifts.Application.Features.Summary.Queries.GetGiftsSummary;
using Gifts.Domain.People;
using Gifts.Domain.Wishlist;

namespace UnitTests.Gifts.Summary;

/// <summary>The gifts summary's lists: the 🔥 wishes per recipient and the upcoming occasions, with a fixed "today".</summary>
public sealed class SummaryListsTests
{
    private static readonly DateOnly Today = new(2026, 11, 26);
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private static SummaryRow Wanted(
        string name, Priority priority = Priority.Normal, DateOnly? occasionDate = null, int createdMinute = 0,
        Recipient recipient = Recipient.Maria, GiftStatus status = GiftStatus.Wanted) =>
        new(Guid.NewGuid(), recipient, name, priority, null, null, status, occasionDate is null ? null : "повод", occasionDate,
            Created.AddMinutes(createdMinute));

    [Fact]
    public void The_HighWishes_AreAtMostFive_NearestOccasionFirst_ThenOldest()
    {
        SummaryRow[] rows =
        [
            Wanted("без даты", Priority.High, createdMinute: 1),
            Wanted("скоро", Priority.High, Today.AddDays(3)),
            Wanted("позже", Priority.High, Today.AddDays(30)),
            Wanted("обычный", Priority.Normal, Today.AddDays(1)),
            Wanted("3", Priority.High, createdMinute: 2),
            Wanted("4", Priority.High, createdMinute: 3),
            Wanted("5", Priority.High, createdMinute: 4),
            Wanted("Виталию", Priority.High, recipient: Recipient.Vitaliy),
        ];

        var maria = SummaryFigures.Build(rows, [], Today).Recipients.Single(r => r.Recipient == Recipient.Maria);

        Assert.Equal(["скоро", "позже", "без даты", "3", "4"], maria.TopWanted.Select(g => g.Name));
    }

    [Fact]
    public void Upcoming_Occasions_AreWantedGifts_FromToday_NearestFirst_AtMostFive()
    {
        SummaryRow[] rows =
        [
            Wanted("вчера", occasionDate: Today.AddDays(-1)),
            Wanted("сегодня", occasionDate: Today),
            Wanted("подарено", occasionDate: Today.AddDays(1), status: GiftStatus.Gifted),
            Wanted("через 2", occasionDate: Today.AddDays(2)),
            Wanted("через 9", occasionDate: Today.AddDays(9)),
            Wanted("через 5", occasionDate: Today.AddDays(5)),
            Wanted("через 7", occasionDate: Today.AddDays(7)),
            Wanted("через 8", occasionDate: Today.AddDays(8)),
            Wanted("без даты"),
        ];

        var upcoming = SummaryFigures.Build(rows, [], Today).Upcoming;

        Assert.Equal(["сегодня", "через 2", "через 5", "через 7", "через 8"], upcoming.Select(o => o.Name));
    }
}

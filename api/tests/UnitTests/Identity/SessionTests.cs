using Identity.Domain.Sessions;

namespace UnitTests.Identity;

/// <summary>Spec identity/001: R-004, R-005, R-007, R-012.</summary>
public sealed class SessionTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 27, 9, 0, 0, TimeSpan.Zero);
    private static readonly MemberProfile Vitaliy = new("vitaliy@example.com", "Віталій", "https://lh3.example/p.png");

    private static Session NewSession() => Session.Start(new string('a', 64), Vitaliy, Start);

    [Fact]
    public void A_NewSession_IsActive_ForThirtyDays()
    {
        var session = NewSession();

        Assert.Equal(Start + TimeSpan.FromDays(30), session.ExpiresAtUtc);
        Assert.True(session.IsActiveAt(Start.AddDays(29)));
        Assert.False(session.IsActiveAt(Start.AddDays(30)));
        Assert.Equal(Vitaliy, session.Member);
    }

    [Fact]
    public void A_Name_IsTrimmed_AndAnEmptyName_FallsBackToTheEmail()
    {
        Assert.Equal("Маша", Session.Start("h", Vitaliy with { Name = "  Маша " }, Start).Name);
        Assert.Equal(Vitaliy.Email, Session.Start("h", Vitaliy with { Name = " " }, Start).Name);
    }

    [Fact]
    public void A_TooLongPictureUrl_IsDropped()
    {
        var session = Session.Start("h", Vitaliy with { PictureUrl = new string('x', Session.MaxPictureUrlLength + 1) }, Start);

        Assert.Null(session.PictureUrl);
    }

    [Fact]
    public void Touch_WithinAnHour_ChangesNothing()
    {
        var session = NewSession();

        Assert.False(session.Touch(Start.AddMinutes(59)));
        Assert.Equal(Start + Session.Lifetime, session.ExpiresAtUtc);
    }

    [Fact]
    public void Touch_AfterAnHour_SlidesTheExpiry()
    {
        var session = NewSession();
        var later = Start.AddDays(20);

        Assert.True(session.Touch(later));
        Assert.Equal(later, session.LastSeenAtUtc);
        Assert.Equal(later + Session.Lifetime, session.ExpiresAtUtc);
        Assert.True(session.IsActiveAt(Start.AddDays(45)));
    }

    [Fact]
    public void Touch_DoesNotRevive_AnExpiredSession()
    {
        var session = NewSession();

        Assert.False(session.Touch(Start.AddDays(31)));
        Assert.False(session.IsActiveAt(Start.AddDays(31)));
    }

    [Fact]
    public void Revoke_EndsTheSession_AndKeepsTheFirstTime()
    {
        var session = NewSession();

        session.Revoke(Start.AddHours(1));
        session.Revoke(Start.AddHours(2));

        Assert.False(session.IsActiveAt(Start.AddHours(1)));
        Assert.Equal(Start.AddHours(1), session.RevokedAtUtc);
        Assert.False(session.Touch(Start.AddHours(3)));
    }

    [Fact]
    public void A_Session_IsStale_ThirtyDaysAfterItEnded()
    {
        var expired = NewSession();
        var revoked = NewSession();
        revoked.Revoke(Start.AddDays(1));

        Assert.False(expired.IsStaleAt(Start.AddDays(59)));
        Assert.True(expired.IsStaleAt(Start.AddDays(61)));
        Assert.False(revoked.IsStaleAt(Start.AddDays(30)));
        Assert.True(revoked.IsStaleAt(Start.AddDays(32)));
        Assert.False(NewSession().IsStaleAt(Start.AddDays(10)));
    }
}

using Todos.Application.Features.Categories.Commands.CreateTodoCategory;
using Todos.Application.Features.Tasks.Queries.ListTodoTasks;
using Todos.Domain.Parties;

namespace UnitTests.Todos.Input;

public sealed class ListAndCategoryValidatorTests
{
    private static readonly ListTodoTasksValidator List = new();
    private static readonly CreateTodoCategoryValidator Category = new();

    private static ListTodoTasksQuery Query(
        string view = "active",
        Guid? categoryId = null,
        bool withoutCategory = false,
        int pageSize = 50,
        DateOnly? dueFrom = null,
        DateOnly? dueTo = null,
        bool withoutDueDate = false,
        DateOnly? completedFrom = null,
        DateOnly? completedTo = null) =>
        new(view, [Assignee.Maria, Assignee.Shared], categoryId, withoutCategory, null,
            dueFrom, dueTo, withoutDueDate, completedFrom, completedTo, 1, pageSize);

    private static readonly DateOnly Day = new(2026, 11, 30);

    [Theory]
    [InlineData("active")]
    [InlineData("archive")]
    public void Both_Views_AreValid(string view)
    {
        Assert.True(List.Validate(Query(view)).IsValid);
    }

    [Fact]
    public void An_Unknown_View_IsInvalid()
    {
        Assert.Contains(List.Validate(Query("trash")).Errors, e => e.PropertyName == "View");
    }

    [Fact]
    public void A_Category_AndWithoutCategory_Together_AreInvalid()
    {
        Assert.False(List.Validate(Query(categoryId: Guid.NewGuid(), withoutCategory: true)).IsValid);
    }

    [Fact]
    public void A_Due_Period_IsValid_AndMayBe_OpenOnEitherSide()
    {
        Assert.True(List.Validate(Query(dueFrom: Day, dueTo: Day.AddDays(6))).IsValid);
        Assert.True(List.Validate(Query(dueTo: Day)).IsValid);
        Assert.True(List.Validate(Query(dueFrom: Day)).IsValid);
        Assert.True(List.Validate(Query(withoutDueDate: true)).IsValid);
    }

    [Fact]
    public void A_Due_Period_EndingBeforeItStarts_IsInvalid()
    {
        Assert.Contains(List.Validate(Query(dueFrom: Day, dueTo: Day.AddDays(-1))).Errors, e => e.PropertyName == "dueTo");
    }

    [Fact]
    public void A_Due_Period_AndWithoutDueDate_Together_AreInvalid()
    {
        Assert.Contains(List.Validate(Query(dueFrom: Day, withoutDueDate: true)).Errors, e => e.PropertyName == "withoutDueDate");
    }

    [Fact]
    public void A_Completion_Period_IsValid_OnlyInTheArchive()
    {
        Assert.True(List.Validate(Query("archive", completedFrom: Day, completedTo: Day)).IsValid);
        Assert.Contains(List.Validate(Query(completedFrom: Day)).Errors, e => e.PropertyName == "completedFrom");
        Assert.Contains(List.Validate(Query("archive", completedFrom: Day, completedTo: Day.AddDays(-1))).Errors, e => e.PropertyName == "completedTo");
    }

    [Fact]
    public void A_Kyiv_Day_StartsAtKyivMidnight_InUtc()
    {
        // Kyiv is UTC+2 in winter and UTC+3 in summer.
        Assert.Equal(new DateTimeOffset(2026, 11, 29, 22, 0, 0, TimeSpan.Zero), global::Todos.Application.KyivCalendar.StartOfDayInKyiv(Day));
        Assert.Equal(new DateTimeOffset(2026, 6, 30, 21, 0, 0, TimeSpan.Zero), global::Todos.Application.KyivCalendar.StartOfDayInKyiv(new DateOnly(2026, 7, 1)));
    }

    [Fact]
    public void A_Page_LargerThan100_IsInvalid()
    {
        Assert.Contains(List.Validate(Query(pageSize: 101)).Errors, e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void A_Category_Needs_AName_OfAtMost50_AndAnEmoji_OfAtMost8()
    {
        Assert.True(Category.Validate(new CreateTodoCategoryCommand("Дача", "🏡")).IsValid);
        Assert.False(Category.Validate(new CreateTodoCategoryCommand("", null)).IsValid);
        Assert.False(Category.Validate(new CreateTodoCategoryCommand(new string('x', 51), null)).IsValid);
        Assert.False(Category.Validate(new CreateTodoCategoryCommand("Дача", "123456789")).IsValid);
    }
}

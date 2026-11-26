using Todos.Application.Features.Categories.Commands.CreateTodoCategory;
using Todos.Application.Features.Tasks.Queries.ListTodoTasks;
using Todos.Domain.Parties;

namespace UnitTests.Todos.Input;

public sealed class ListAndCategoryValidatorTests
{
    private static readonly ListTodoTasksValidator List = new();
    private static readonly CreateTodoCategoryValidator Category = new();

    private static ListTodoTasksQuery Query(string view = "active", Guid? categoryId = null, bool withoutCategory = false, int pageSize = 50) =>
        new(view, [Assignee.Maria, Assignee.Shared], categoryId, withoutCategory, null, 1, pageSize);

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

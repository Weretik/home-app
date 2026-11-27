using System;
using Shopping.Domain.Departments;

namespace UnitTests.Shopping.Domain;

public sealed class DepartmentTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 26, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_Department_HasATrimmedName_AndAnIcon()
    {
        var department = Department.Create("  Рынок ", " 🧺 ", 50, Now).Value;

        Assert.Equal("Рынок", department.Name);
        Assert.Equal("🧺", department.Icon);
        Assert.Equal(50, department.SortOrder);
        Assert.False(department.IsFallback);
        Assert.True(department.EnsureDeletable().IsSuccess);
    }

    [Theory]
    [InlineData("", null, "shopping.department_name_required")]
    [InlineData("12345678901234567890123456789012345678901", null, "shopping.department_name_too_long")]
    [InlineData("Рынок", "123456789", "shopping.department_icon_too_long")]
    public void Invalid_Names_AndIcons_AreRefused(string name, string? icon, string error)
    {
        Assert.Equal(error, Department.Create(name, icon, 10, Now).Error!.Code);
    }

    [Fact]
    public void A_Rename_KeepsThePlace()
    {
        var department = Department.Create("Рынок", null, 50, Now).Value;

        Assert.True(department.Rename("Базар", "🍎").IsSuccess);

        Assert.Equal("Базар", department.Name);
        Assert.Equal("🍎", department.Icon);
        Assert.Equal(50, department.SortOrder);
    }

    [Fact]
    public void Keywords_AreStoredNormalised()
    {
        var keyword = DepartmentKeyword.Create(" Зубн  Паст ", new DepartmentId(Guid.NewGuid()), learned: false, Now).Value;

        Assert.Equal("зубн паст", keyword.Keyword);
        Assert.False(keyword.Learned);
    }

    [Theory]
    [InlineData("я", "shopping.keyword_too_short")]
    [InlineData("   ", "shopping.keyword_too_short")]
    public void Keywords_HaveTwoTo60Characters(string text, string error)
    {
        Assert.Equal(error, DepartmentKeyword.Create(text, new DepartmentId(Guid.NewGuid()), false, Now).Error!.Code);
        Assert.Equal("shopping.keyword_too_long", DepartmentKeyword.Create(new string('я', 61), new DepartmentId(Guid.NewGuid()), false, Now).Error!.Code);
        Assert.False(DepartmentKeyword.IsValid(text));
    }

    [Fact]
    public void Learning_MovesTheKeyword_AndMarksItLearned()
    {
        var keyword = DepartmentKeyword.Create("штуковина", new DepartmentId(Guid.NewGuid()), false, Now).Value;
        var grocery = new DepartmentId(Guid.NewGuid());

        keyword.Learn(grocery);

        Assert.Equal(grocery, keyword.DepartmentId);
        Assert.True(keyword.Learned);
    }
}

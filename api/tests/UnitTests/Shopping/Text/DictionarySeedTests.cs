using Shopping.Domain.Departments;
using Shopping.Domain.Matching;
using Shopping.Infrastructure.Persistence.Reference;

namespace UnitTests.Shopping.Text;

/// <summary>The seeded «Справочник» and the matcher together put the bot's examples where the bot did.</summary>
public sealed class DictionarySeedTests
{
    private static readonly DepartmentMatcher Matcher = new(
        DictionarySeed.Keywords
            .OrderBy(k => DictionarySeed.Departments.Single(d => d.Id == k.DepartmentId).SortOrder)
            .ThenBy(k => k.Keyword, StringComparer.Ordinal)
            .Select(k => new KeywordRule(k.Keyword, new DepartmentId(k.DepartmentId))),
        new DepartmentId(DictionarySeed.Departments.Single(d => d.IsFallback).Id));

    [Fact]
    public void The_Sheet_Has13Departments_InStoreOrder_WithOneFallback_Last()
    {
        Assert.Equal(13, DictionarySeed.Departments.Count);
        Assert.Equal(("🥦", "Овощи и фрукты", 10), (DictionarySeed.Departments[0].Icon, DictionarySeed.Departments[0].Name, DictionarySeed.Departments[0].SortOrder));
        var last = DictionarySeed.Departments[^1];
        Assert.Equal(("🧺", "Другое", true), (last.Icon, last.Name, last.IsFallback));
        Assert.Single(DictionarySeed.Departments, d => d.IsFallback);
    }

    [Fact]
    public void Keywords_AreNormalised_Unique_AndHaveStableIds()
    {
        Assert.Equal(396, DictionarySeed.Keywords.Count);
        Assert.Equal(DictionarySeed.Keywords.Count, DictionarySeed.Keywords.Select(k => k.Keyword).Distinct().Count());
        Assert.DoesNotContain(DictionarySeed.Keywords, k => k.Keyword.Contains('ё', StringComparison.Ordinal));
        Assert.Equal(SeedIds.Keyword("молок"), DictionarySeed.Keywords.Single(k => k.Keyword == "молок").Id);
        Assert.Equal(new Guid("29aeaa2c-a96b-5320-a72b-e96a8e09d7d6"), SeedIds.Department("Другое"));
    }

    [Fact]
    public void Ids_AreVersion5Uuids()
    {
        var id = SeedIds.Department("Хлеб").ToString();

        Assert.Equal('5', id[14]);
        Assert.Contains(id[19], "89ab");
    }

    [Theory]
    [InlineData("Молоко", "Молочное и яйца")]
    [InlineData("Хлеб", "Хлеб")]
    [InlineData("Яйца", "Молочное и яйца")]
    [InlineData("Сыр", "Сыр и колбаса")]
    [InlineData("Картошки", "Овощи и фрукты")]
    [InlineData("Зубная паста", "Гигиена и аптека")]
    [InlineData("Паста", "Бакалея")]
    [InlineData("Масло подсолнечное", "Бакалея")]
    [InlineData("Масло сливочное", "Молочное и яйца")]
    [InlineData("Соль для посудомойки", "Бытовая химия")]
    [InlineData("Корм для кошек", "Животным")]
    [InlineData("Мёд", "Бакалея")]
    [InlineData("Штуковина", "Другое")]
    public void The_BotsExamples_LandInTheirDepartments(string name, string department)
    {
        var id = Matcher.Match(name);

        Assert.Equal(department, DictionarySeed.Departments.Single(d => d.Id == id.Value).Name);
    }
}

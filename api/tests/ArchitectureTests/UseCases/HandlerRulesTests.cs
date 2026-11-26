using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using Mediator;

namespace ArchitectureTests.UseCases;

/// <summary>
/// What a handler may do, checked on the code: queries never write and read without tracking or through a
/// projection, a command saves once, business rules return results instead of throwing, and a paged list never
/// serves more than 100 rows a page.
/// </summary>
public sealed partial class HandlerRulesTests
{
    private const int MaxPageSize = 100;

    private static IEnumerable<string> UseCaseFiles(string kind) =>
        SourceFiles.Under("src/Modules")
            .Where(file => SourceFiles.Relative(file).Contains($"/Features/", StringComparison.Ordinal)
                && SourceFiles.Relative(file).Contains($"/{kind}/", StringComparison.Ordinal));

    [Fact]
    public void A_Query_NeverChangesState()
    {
        var violations =
            from file in UseCaseFiles("Queries")
            let text = File.ReadAllText(file)
            where Write().IsMatch(text)
            select $"{SourceFiles.Relative(file)}: {Write().Match(text).Value}";

        Assert.Empty(violations);
    }

    [Fact]
    public void A_Query_ReadsWithoutTracking_OrThroughAProjection()
    {
        var violations =
            from file in UseCaseFiles("Queries")
            let text = File.ReadAllText(file)
            from Match read in SetAccess().Matches(text)
            let statement = text[read.Index..(text.IndexOf(';', read.Index) + 1)]
            where !statement.Contains(".AsNoTracking()", StringComparison.Ordinal) && !statement.Contains(".Select(", StringComparison.Ordinal)
            select $"{SourceFiles.Relative(file)}: {read.Value}";

        Assert.Empty(violations);
    }

    [Fact]
    public void A_Command_SavesOnce()
    {
        var violations =
            from file in UseCaseFiles("Commands")
            where file.EndsWith("Handler.cs", StringComparison.Ordinal)
            let saves = Save().Matches(File.ReadAllText(file)).Count
            where saves > 1
            select $"{SourceFiles.Relative(file)}: {saves} saves";

        Assert.Empty(violations);
    }

    [Fact]
    public void Business_Rules_ReturnResults_InsteadOfThrowing()
    {
        var violations =
            from module in Repository.Modules
            from layer in new[] { "Domain", "Application" }
            from file in SourceFiles.Under($"src/Modules/{module}/{module}.{layer}")
            let text = File.ReadAllText(file)
            where Throw().IsMatch(text)
            select $"{SourceFiles.Relative(file)}: {Throw().Match(text).Value}";

        Assert.Empty(violations);
    }

    [Fact]
    public void A_PagedList_ServesAtMost100RowsAPage()
    {
        var violations = new List<string>();
        foreach (var module in Repository.Modules)
        {
            var types = Repository.Assembly($"{module}.Application").GetTypes();
            var paged = types.Where(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
                && t.GetProperty("PageSize")?.PropertyType == typeof(int));
            foreach (var query in paged)
            {
                var validatorType = types.FirstOrDefault(t => typeof(IValidator<>).MakeGenericType(query).IsAssignableFrom(t));
                if (validatorType is null)
                {
                    violations.Add($"{query.Name}: no validator");
                    continue;
                }

                var validator = (IValidator)Activator.CreateInstance(validatorType, nonPublic: true)!;
                if (!PageSizeErrors(validator, query, MaxPageSize + 1) || PageSizeErrors(validator, query, MaxPageSize) || !PageSizeErrors(validator, query, 0))
                    violations.Add($"{query.Name}: page size must be 1–{MaxPageSize}");
            }
        }

        Assert.Empty(violations);
    }

    private static bool PageSizeErrors(IValidator validator, Type query, int pageSize)
    {
        var constructor = query.GetConstructors().OrderByDescending(c => c.GetParameters().Length).First();
        var arguments = constructor.GetParameters()
            .Select(p => p.Name == "PageSize" ? pageSize : DefaultFor(p))
            .ToArray();
        var instance = constructor.Invoke(arguments);
        var context = (IValidationContext)Activator.CreateInstance(typeof(ValidationContext<>).MakeGenericType(query), instance)!;
        return validator.Validate(context).Errors.Any(e => e.PropertyName == "PageSize");
    }

    private static object? DefaultFor(ParameterInfo parameter) =>
        parameter.ParameterType.IsValueType && Nullable.GetUnderlyingType(parameter.ParameterType) is null
            ? Activator.CreateInstance(parameter.ParameterType)
            : null;

    [GeneratedRegex(@"SaveChangesAsync\(|Execute(Update|Delete)Async\(|\bdb\.\w+\.(Add|AddRange|Remove|RemoveRange|Update|Attach)\(")]
    private static partial Regex Write();

    [GeneratedRegex(@"(?<!Join\()\bdb\.\w+")]
    private static partial Regex SetAccess();

    [GeneratedRegex(@"SaveChangesAsync\(|Execute(Update|Delete)Async\(")]
    private static partial Regex Save();

    [GeneratedRegex(@"\bthrow\b(?!Helper)")]
    private static partial Regex Throw();
}

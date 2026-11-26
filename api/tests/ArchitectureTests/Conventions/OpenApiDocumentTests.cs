using System.Text.Json;
using System.Text.RegularExpressions;

namespace ArchitectureTests.Conventions;

/// <summary>
/// The committed OpenAPI document is what the frontend generates its client from, so every operation has a
/// unique camelCase id and a summary, and numbers are plain JSON numbers.
/// </summary>
public sealed partial class OpenApiDocumentTests
{
    private static readonly JsonElement Document =
        JsonDocument.Parse(File.ReadAllText(Path.Combine(Repository.Root, "docs/contracts/openapi.json"))).RootElement;

    private static IEnumerable<(string Route, JsonElement Operation)> Operations() =>
        from path in Document.GetProperty("paths").EnumerateObject()
        from operation in path.Value.EnumerateObject()
        select ($"{operation.Name.ToUpperInvariant()} {path.Name}", operation.Value);

    [Fact]
    public void Every_Operation_HasAUniqueCamelCaseId_AndASummary()
    {
        var operations = Operations().ToList();
        var ids = operations.Select(o => o.Operation.TryGetProperty("operationId", out var id) ? id.GetString() : null).ToList();

        Assert.All(operations, o => Assert.True(o.Operation.TryGetProperty("summary", out _), $"{o.Route} has no summary"));
        Assert.All(ids, id => Assert.Matches(CamelCase(), id ?? string.Empty));
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Numbers_AreNotAlsoStrings()
    {
        var json = Document.GetProperty("components").GetRawText();

        Assert.DoesNotContain("\"number\",\"string\"", json.Replace(" ", string.Empty).Replace("\n", string.Empty));
    }

    [GeneratedRegex("^[a-z][a-zA-Z0-9]+$")]
    private static partial Regex CamelCase();
}

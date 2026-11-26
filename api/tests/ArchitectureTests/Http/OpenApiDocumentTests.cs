using System.Text.Json;
using System.Text.RegularExpressions;

namespace ArchitectureTests.Http;

/// <summary>
/// The committed OpenAPI document is what the frontend generates its client from, so every operation has a
/// unique camelCase id and a summary, numbers are plain JSON numbers, every error is a problem response, bodies are
/// endpoint request records, and the write conventions (rate limit, Idempotency-Key, If-Match) are visible in it.
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

    /// <summary>Creates that are protected otherwise, with the reason.</summary>
    private static readonly Dictionary<string, string> CreatesWithoutIdempotencyKey = new()
    {
        ["recordPlannedExpense"] = "guarded by the plan's version; a repeat finds the plan gone (ADR 0028)",
        ["createTodoCategory"] = "a repeat hits the unique name and answers 409 (ADR 0030)",
        ["addTodoTaskStep"] = "a duplicated step is visible in the task and removed with one tap (ADR 0030)",
    };

    /// <summary>Edits that carry no version, with the reason.</summary>
    private static readonly Dictionary<string, string> EditsWithoutVersion = new()
    {
        ["renameGiftCategory"] = "a rename is a target value and a category has no other field to lose (ADR 0024)",
        ["setShoppingItemStatus"] = "a tick is a target value, so two people ticking the same item cannot undo each other (ADR 0031)",
        ["updateTodoTaskStep"] = "a step tick or rename is a target value; quick ticks must never conflict (ADR 0030, R-010)",
        ["reorderShoppingDepartments"] = "the whole order is a target value; a concurrent save still meets the row version (ADR 0031)",
    };

    private static IEnumerable<string> ResponseCodes(JsonElement operation) =>
        operation.GetProperty("responses").EnumerateObject().Select(r => r.Name);

    private static IEnumerable<string> HeaderParameters(JsonElement operation) =>
        operation.TryGetProperty("parameters", out var parameters)
            ? parameters.EnumerateArray().Where(p => p.GetProperty("in").GetString() == "header").Select(p => p.GetProperty("name").GetString()!)
            : [];

    private static bool IsWrite(string route) => !route.StartsWith("GET ", StringComparison.Ordinal);

    [Fact]
    public void Every_Error_IsAProblemResponse()
    {
        var violations =
            from o in Operations()
            from response in o.Operation.GetProperty("responses").EnumerateObject()
            where response.Name[0] is '4' or '5'
            where !response.Value.TryGetProperty("content", out var content) || !content.TryGetProperty("application/problem+json", out _)
            select $"{o.Route} {response.Name}";

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_SuccessWithABody_HasASchema()
    {
        var violations =
            from o in Operations()
            from response in o.Operation.GetProperty("responses").EnumerateObject()
            where response.Name[0] == '2' && response.Name != "204"
            where !response.Value.TryGetProperty("content", out var content)
                || !content.EnumerateObject().All(c => c.Value.TryGetProperty("schema", out _))
            select $"{o.Route} {response.Name}";

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_Operation_WithInput_ListsTheValidationProblem()
    {
        var violations =
            from o in Operations()
            let hasInput = o.Operation.TryGetProperty("requestBody", out _)
                || (o.Operation.TryGetProperty("parameters", out var parameters) && parameters.GetArrayLength() > 0)
            where hasInput && !ResponseCodes(o.Operation).Contains("400")
            select o.Route;

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_Write_IsRateLimited_AndLists429()
    {
        var violations = Operations()
            .Where(o => IsWrite(o.Route) && !ResponseCodes(o.Operation).Contains("429"))
            .Select(o => o.Route);

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_Create_TakesAnIdempotencyKey_AndEvery_Edit_TheVersion()
    {
        var operations = Operations().ToList();
        var creates = operations
            .Where(o => o.Route.StartsWith("POST ", StringComparison.Ordinal) && ResponseCodes(o.Operation).Contains("201"))
            .Where(o => !CreatesWithoutIdempotencyKey.ContainsKey(o.Operation.GetProperty("operationId").GetString()!))
            .Where(o => !HeaderParameters(o.Operation).Contains("Idempotency-Key"))
            .Select(o => $"{o.Route} has no Idempotency-Key");
        var edits = operations
            .Where(o => o.Route.StartsWith("PUT ", StringComparison.Ordinal) && !HeaderParameters(o.Operation).Contains("If-Match"))
            .Where(o => !EditsWithoutVersion.ContainsKey(o.Operation.GetProperty("operationId").GetString()!))
            .Select(o => $"{o.Route} has no If-Match");

        Assert.Empty(creates.Concat(edits));
    }

    [Fact]
    public void Request_Bodies_AreEndpointRequestRecords_NotCommands()
    {
        var violations =
            from o in Operations()
            where o.Operation.TryGetProperty("requestBody", out _)
            let body = o.Operation.GetProperty("requestBody").GetRawText()
            let schema = SchemaReference().Match(body).Groups["name"].Value
            where !schema.EndsWith("Request", StringComparison.Ordinal)
            select $"{o.Route}: {schema}";

        Assert.Empty(violations);
    }

    [GeneratedRegex("#/components/schemas/(?<name>[A-Za-z0-9]+)")]
    private static partial Regex SchemaReference();

    [GeneratedRegex("^[a-z][a-zA-Z0-9]+$")]
    private static partial Regex CamelCase();
}

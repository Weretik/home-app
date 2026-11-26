using System.Reflection;
using System.Text.RegularExpressions;
using Mediator;

namespace ArchitectureTests.Http;

/// <summary>
/// Minimal API endpoints: one per file named after it, every write opts into a rate limit, and an endpoint takes an
/// HTTP request record and maps it to the command; it never binds a command or a query straight from HTTP.
/// </summary>
public sealed partial class EndpointRulesTests
{
    /// <summary>Writes without a rate limit, with the reason.</summary>
    private static readonly Dictionary<string, string> UnlimitedWrites = new()
    {
        ["OutboxTickEndpoint.cs"] = "called by the scheduler with a secret key, hidden from OpenAPI (ADR 0006)",
        ["NotificationsTickEndpoint.cs"] = "called by the scheduler with a secret key, hidden from OpenAPI (ADR 0034)",
        ["TelegramWebhookEndpoint.cs"] = "called only by Telegram with the secret header, hidden from OpenAPI (ADR 0033)",
    };

    private static IEnumerable<(string File, string Text)> EndpointFiles() =>
        SourceFiles.Under("src")
            .Select(file => (file, text: File.ReadAllText(file)))
            .Where(x => Map().IsMatch(x.text));

    [Fact]
    public void A_File_MapsOneEndpoint_AndIsNamedAfterIt()
    {
        var violations = EndpointFiles()
            .Where(x => SourceFiles.Relative(x.File).Contains("/Endpoints/", StringComparison.Ordinal))
            .Where(x => Map().Matches(x.Text).Count != 1 || !x.File.EndsWith("Endpoint.cs", StringComparison.Ordinal))
            .Select(x => $"{SourceFiles.Relative(x.File)}: {Map().Matches(x.Text).Count} endpoints");

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_Write_OptsIntoARateLimit()
    {
        var violations = EndpointFiles()
            .Where(x => WriteMap().IsMatch(x.Text) && !x.Text.Contains(".RequireRateLimiting(", StringComparison.Ordinal))
            .Where(x => !UnlimitedWrites.ContainsKey(Path.GetFileName(x.File)))
            .Select(x => SourceFiles.Relative(x.File));

        Assert.Empty(violations);
    }

    [Fact]
    public void An_Endpoint_NeverBindsACommandOrAQuery_FromHttp()
    {
        var violations =
            from module in Repository.Modules
            from type in Repository.Assembly($"{module}.Api").GetTypes()
            from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            from parameter in method.GetParameters()
            where typeof(IMessage).IsAssignableFrom(parameter.ParameterType)
            select $"{type.Name}.{method.Name}({parameter.ParameterType.Name})";

        Assert.Empty(violations);
    }

    [GeneratedRegex(@"\.Map(Get|Post|Put|Delete|Patch)\(")]
    private static partial Regex Map();

    [GeneratedRegex(@"\.Map(Post|Put|Delete|Patch)\(")]
    private static partial Regex WriteMap();
}

using System.Text.Json;

namespace ArchitectureTests.Hosting;

/// <summary>
/// Committed configuration holds no secrets outside local development: connection strings and keys come from
/// environment variables or user secrets. Production never migrates on startup; migrations are a deploy step
/// (ADR 0016).
/// </summary>
public sealed class ConfigurationRulesTests
{
    private static readonly string[] SecretNames = ["Key", "Password", "Secret", "Token", "ConnectionString"];

    private static IEnumerable<string> CommittedSettings() =>
        Directory.EnumerateFiles(Path.Combine(Repository.Root, "src"), "appsettings*.json", SearchOption.AllDirectories)
            .Where(file => !SourceFiles.Relative(file).Split('/').Any(part => part is "bin" or "obj"))
            .Where(file => !Path.GetFileName(file).Equals("appsettings.Development.json", StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void Committed_Settings_HoldNoSecrets()
    {
        var violations =
            from file in CommittedSettings()
            from setting in Flatten(JsonDocument.Parse(File.ReadAllText(file)).RootElement, string.Empty)
            where setting.Path.StartsWith("ConnectionStrings:", StringComparison.Ordinal)
                || SecretNames.Any(name => setting.Path.Split(':')[^1].EndsWith(name, StringComparison.Ordinal))
            where !string.IsNullOrEmpty(setting.Value)
            select $"{SourceFiles.Relative(file)}: {setting.Path}";

        Assert.Empty(violations);
    }

    [Fact]
    public void Production_DoesNotMigrateOnStartup()
    {
        var violations =
            from file in CommittedSettings()
            from setting in Flatten(JsonDocument.Parse(File.ReadAllText(file)).RootElement, string.Empty)
            where setting.Path == "Database:InitializeOnStartup" && setting.Value == "True"
            select SourceFiles.Relative(file);

        Assert.Empty(violations);
    }

    private static IEnumerable<(string Path, string? Value)> Flatten(JsonElement element, string path) =>
        element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().SelectMany(p => Flatten(p.Value, path.Length == 0 ? p.Name : $"{path}:{p.Name}")),
            JsonValueKind.Array => element.EnumerateArray().SelectMany((e, i) => Flatten(e, $"{path}:{i}")),
            JsonValueKind.String => [(path, element.GetString())],
            _ => [(path, element.ValueKind == JsonValueKind.Null ? null : element.GetRawText() is "true" ? "True" : element.GetRawText())],
        };
}

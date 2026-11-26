using System.Text.RegularExpressions;

namespace ArchitectureTests.Structure;

/// <summary>
/// The records that explain the code stay findable: every ADR has a status and is in the decisions index, every
/// feature specification (SDD) is in the specs index, and every module has its module page.
/// </summary>
public sealed partial class DocumentationIndexTests
{
    private static string Docs(string path) => Path.Combine(Repository.Root, "docs", path);

    [Fact]
    public void Every_Decision_HasAStatus_AndIsInTheIndex()
    {
        var index = File.ReadAllText(Docs("decisions/README.md"));

        var violations =
            from file in Directory.EnumerateFiles(Docs("decisions"), "*.md")
            let name = Path.GetFileName(file)
            where DecisionFile().IsMatch(name)
            where !index.Contains($"({name})", StringComparison.Ordinal) || !File.ReadAllText(file).Contains("Status: ", StringComparison.Ordinal)
            select name;

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_Specification_IsInTheIndex()
    {
        var specs = Docs("specs");
        if (!Directory.Exists(specs))
            return;
        var index = File.ReadAllText(Path.Combine(specs, "README.md"));

        var violations =
            from module in Directory.EnumerateDirectories(specs)
            from feature in Directory.EnumerateDirectories(module)
            let link = $"{Path.GetFileName(module)}/{Path.GetFileName(feature)}/README.md"
            where !File.Exists(Path.Combine(feature, "README.md")) || !index.Contains($"({link})", StringComparison.Ordinal)
            select link;

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_Module_HasItsModulePage()
    {
        var missing = Repository.Modules
            .Where(module => !File.Exists(Docs($"modules/{module.ToLowerInvariant()}.md")));

        Assert.Empty(missing);
    }

    [GeneratedRegex(@"^\d{4}-.+\.md$")]
    private static partial Regex DecisionFile();
}

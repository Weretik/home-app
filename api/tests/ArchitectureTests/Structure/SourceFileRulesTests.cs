using System.Text.RegularExpressions;

namespace ArchitectureTests.Structure;

/// <summary>
/// One type per file, and the file has the type's name (a generic <c>Result&lt;T&gt;</c> lives in <c>ResultOfT.cs</c>).
/// Types nested inside another type are part of it. Checked on the source text: a top-level declaration starts at
/// the beginning of a line under a file-scoped namespace.
/// </summary>
public sealed partial class SourceFileRulesTests
{
    public static TheoryData<string> Roots => new("src", "tests");

    [Theory]
    [MemberData(nameof(Roots))]
    public void A_File_DeclaresOneType_NamedLikeTheFile(string root)
    {
        var violations =
            from file in SourceFiles.Under(root)
            let names = TopLevelType().Matches(File.ReadAllText(file)).Select(m => m.Groups["name"].Value).Distinct().ToList()
            let expected = Path.GetFileNameWithoutExtension(file)
            where names.Count != 1 || (names[0] != expected && $"{names[0]}OfT" != expected)
            select $"{SourceFiles.Relative(file)}: {string.Join(", ", names)}";

        Assert.Empty(violations);
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void A_File_UsesAFileScopedNamespace_ThatFollowsItsFolder(string root)
    {
        var violations =
            from file in SourceFiles.Under(root)
            where !file.EndsWith("Program.cs", StringComparison.Ordinal)
            let match = FileScopedNamespace().Match(File.ReadAllText(file))
            let folder = Path.GetDirectoryName(SourceFiles.Relative(file))!.Replace('\\', '/')
            where !match.Success || !NamespaceFollows(match.Groups["ns"].Value, folder)
            select $"{SourceFiles.Relative(file)}: {(match.Success ? match.Groups["ns"].Value : "no file-scoped namespace")}";

        Assert.Empty(violations);
    }

    /// <summary>The namespace ends with the folders below the project folder, in order.</summary>
    private static bool NamespaceFollows(string ns, string folder)
    {
        var parts = folder.Split('/');
        var project = Enumerable.Range(0, parts.Length)
            .LastOrDefault(i => File.Exists(Path.Combine(Repository.Root, string.Join('/', parts[..(i + 1)]), $"{parts[i]}.csproj")), -1);
        if (project < 0)
            return false;
        var expected = string.Join('.', parts[project..]);
        return ns == expected;
    }

    [GeneratedRegex(@"^(?:\[[^\]]*\]\s*)?(?:(?:public|internal|private|protected|file|sealed|static|abstract|partial|readonly|ref|unsafe)\s+)*(?:class|record\s+struct|record\s+class|record|struct|interface|enum)\s+(?<name>\w+)", RegexOptions.Multiline)]
    private static partial Regex TopLevelType();

    [GeneratedRegex(@"^namespace\s+(?<ns>[\w.]+)\s*;", RegexOptions.Multiline)]
    private static partial Regex FileScopedNamespace();
}

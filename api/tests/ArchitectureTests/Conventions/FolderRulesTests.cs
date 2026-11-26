
namespace ArchitectureTests.Conventions;

/// <summary>
/// The folder rules: a folder holds at most six code files, then it is split by meaning into named subfolders;
/// no folder holds a single code file; folders are named by feature or concern, never by kind of class or as a
/// dump. Generated migrations are exempt.
/// </summary>
public sealed class FolderRulesTests
{
    private const int MaxCodeFiles = 6;

    private static readonly string[] ForbiddenNames =
    [
        "Helpers", "Utils", "Utilities", "Common", "Shared", "Misc", "Services", "Managers", "Entities", "Enums",
        "ValueObjects", "Interfaces", "Models", "Dtos", "DTOs", "Constants", "Exceptions", "Repositories",
        "Validators", "Handlers", "Extensions",
    ];

    public static TheoryData<string> Roots => new("src", "tests");

    [Theory]
    [MemberData(nameof(Roots))]
    public void A_Folder_HoldsAtMostSixCodeFiles(string root)
    {
        var violations = CodeFolders(root)
            .Select(folder => (folder, count: Directory.GetFiles(folder, "*.cs").Length))
            .Where(x => x.count > MaxCodeFiles)
            .Select(x => $"{Relative(x.folder)}: {x.count} files, split it into named subfolders");

        Assert.Empty(violations);
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void No_Folder_HoldsASingleCodeFile(string root)
    {
        var violations = CodeFolders(root)
            .Where(folder => !IsProjectFolder(folder))
            .Where(folder => Directory.GetFiles(folder, "*.cs").Length == 1 && Directory.GetDirectories(folder).Length == 0)
            .Select(Relative);

        Assert.Empty(violations);
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void Folders_AreNamedByMeaning_NotByKindOfClass(string root)
    {
        var violations = CodeFolders(root)
            .Where(folder => ForbiddenNames.Contains(Path.GetFileName(folder), StringComparer.OrdinalIgnoreCase))
            .Select(Relative);

        Assert.Empty(violations);
    }

    private static IEnumerable<string> CodeFolders(string root) =>
        Directory.EnumerateDirectories(Path.Combine(Repository.Root, root), "*", SearchOption.AllDirectories)
            .Where(folder => !Relative(folder).Split('/').Any(part => part is "bin" or "obj" or "Migrations" or "Properties"));

    private static bool IsProjectFolder(string folder) => Directory.GetFiles(folder, "*.csproj").Length > 0;

    private static string Relative(string folder) => Path.GetRelativePath(Repository.Root, folder).Replace('\\', '/');
}

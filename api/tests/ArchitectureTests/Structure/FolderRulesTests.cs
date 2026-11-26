
namespace ArchitectureTests.Structure;

/// <summary>
/// The folder rules: a folder holds at most six code files, then it is split by meaning into named subfolders;
/// no folder holds a single code file; no folder is empty or only wraps one subfolder; folders are named by feature
/// or concern, never by kind of class or as a dump. Generated migrations are exempt.
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

    /// <summary>
    /// Levels of the documented layout that may hold one subfolder: a use case under <c>Commands/</c> or
    /// <c>Queries/</c>, a feature with only queries or only commands, and the solution levels above the projects.
    /// </summary>
    private static readonly string[] LayoutLevels = ["Commands", "Queries", "Jobs", "Adapters", "Integrations", "Modules"];

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
    public void No_Folder_IsEmpty(string root)
    {
        var violations = CodeFolders(root)
            .Where(folder => !Directory.EnumerateFileSystemEntries(folder).Any())
            .Select(Relative);

        Assert.Empty(violations);
    }

    [Theory]
    [MemberData(nameof(Roots))]
    public void No_Folder_OnlyWrapsOneSubfolder(string root)
    {
        var violations = CodeFolders(root)
            .Where(folder => Directory.GetFiles(folder).Length == 0)
            .Select(folder => (folder, subfolders: Subfolders(folder)))
            .Where(x => x.subfolders.Length == 1)
            .Where(x => !LayoutLevels.Contains(Path.GetFileName(x.folder)))
            .Where(x => !LayoutLevels.Contains(Path.GetFileName(x.subfolders[0])))
            .Where(x => !IsProjectFolder(x.subfolders[0]))
            .Select(x => $"{Relative(x.folder)} only holds {Path.GetFileName(x.subfolders[0])}/");

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

    private static string[] Subfolders(string folder) =>
        Directory.GetDirectories(folder).Where(sub => Path.GetFileName(sub) is not ("bin" or "obj" or "Properties")).ToArray();

    private static bool IsProjectFolder(string folder) => Directory.GetFiles(folder, "*.csproj").Length > 0;

    private static string Relative(string folder) => Path.GetRelativePath(Repository.Root, folder).Replace('\\', '/');
}

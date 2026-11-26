using System.Xml.Linq;

namespace ArchitectureTests.Structure;

/// <summary>
/// Every project lives under <c>src/</c> or <c>tests/</c>, is in the solution, and sits in the solution folder that
/// mirrors its place on disk; shared build settings come from <c>Directory.Build.props</c>, and package versions only
/// from <c>Directory.Packages.props</c>.
/// </summary>
public sealed class SolutionLayoutTests
{
    private static readonly XDocument Solution = XDocument.Load(Path.Combine(Repository.Root, "HomeApp.slnx"));

    private static IEnumerable<string> ProjectsOnDisk() =>
        Directory.EnumerateFiles(Repository.Root, "*.csproj", SearchOption.AllDirectories)
            .Select(SourceFiles.Relative)
            .Where(path => !path.Split('/').Any(part => part is "bin" or "obj"));

    [Fact]
    public void Every_Project_IsUnderSrcOrTests_AndInTheSolution()
    {
        var listed = Solution.Descendants("Project").Select(p => p.Attribute("Path")!.Value.Replace('\\', '/')).ToHashSet();

        var violations = ProjectsOnDisk()
            .Where(path => !(path.StartsWith("src/", StringComparison.Ordinal) || path.StartsWith("tests/", StringComparison.Ordinal)) || !listed.Contains(path))
            .Concat(listed.Where(path => !File.Exists(Path.Combine(Repository.Root, path))).Select(path => $"{path} (listed, missing)"));

        Assert.Empty(violations);
    }

    [Fact]
    public void Solution_Folders_MirrorTheDisk()
    {
        var violations =
            from folder in Solution.Descendants("Folder")
            let name = folder.Attribute("Name")!.Value.Trim('/')
            from project in folder.Elements("Project")
            let path = project.Attribute("Path")!.Value.Replace('\\', '/')
            let parent = Path.GetDirectoryName(Path.GetDirectoryName(path))!.Replace('\\', '/')
            where parent != name
            select $"{path} is in /{name}/";

        Assert.Empty(violations);
    }

    [Fact]
    public void Package_Versions_LiveOnlyInDirectoryPackagesProps()
    {
        var central = XDocument.Load(Path.Combine(Repository.Root, "Directory.Packages.props"));
        var versions = central.Descendants("PackageVersion").Select(p => p.Attribute("Include")!.Value).ToHashSet();

        var violations =
            from project in ProjectsOnDisk()
            from reference in XDocument.Load(Path.Combine(Repository.Root, project)).Descendants("PackageReference")
            let name = reference.Attribute("Include")!.Value
            where reference.Attribute("Version") is not null || reference.Attribute("VersionOverride") is not null || !versions.Contains(name)
            select $"{project}: {name}";

        Assert.Equal("true", central.Descendants("ManagePackageVersionsCentrally").Single().Value);
        Assert.True(File.Exists(Path.Combine(Repository.Root, "Directory.Build.props")));
        Assert.Empty(violations);
    }
}

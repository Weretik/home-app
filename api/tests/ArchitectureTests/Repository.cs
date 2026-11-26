using System.Reflection;
using System.Xml.Linq;

namespace ArchitectureTests;

/// <summary>Finds the backend folder and reads the solution's projects and assemblies.</summary>
internal static class Repository
{
    public static readonly string Root = FindRoot();

    /// <summary>Every folder under <c>src/Modules</c> is a module; a new module is covered as soon as it exists.</summary>
    public static readonly string[] Modules = FindModules();

    public static readonly string[] Layers = ["Domain", "Application", "Infrastructure", "Api", "Contracts"];

    /// <summary>The project files under a folder; none while the folder does not exist yet (no modules).</summary>
    public static IEnumerable<string> ProjectFiles(string folder)
    {
        var path = Path.Combine(Root, folder);
        return Directory.Exists(path)
            ? Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories)
            : [];
    }

    /// <summary>The names of the projects a project file references directly.</summary>
    public static IReadOnlyList<string> ProjectReferences(string projectFile) =>
        XDocument.Load(projectFile)
            .Descendants("ProjectReference")
            .Select(r => Path.GetFileNameWithoutExtension(r.Attribute("Include")!.Value.Replace('\\', '/')))
            .ToList();

    public static Assembly Assembly(string name) => System.Reflection.Assembly.Load(name);

    private static string[] FindModules()
    {
        var modules = Path.Combine(Root, "src", "Modules");
        return Directory.Exists(modules)
            ? Directory.GetDirectories(modules).Select(Path.GetFileName).OfType<string>().Order().ToArray()
            : [];
    }

    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "HomeApp.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("HomeApp.slnx not found.");
    }
}

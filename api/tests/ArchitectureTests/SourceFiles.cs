namespace ArchitectureTests;

/// <summary>The hand-written C# sources of the backend, for the rules that are checked on the text of the code.</summary>
internal static class SourceFiles
{
    private static readonly string[] Skipped = ["bin", "obj", "Migrations"];

    /// <summary>Every <c>.cs</c> file under <paramref name="folder"/> (relative to the backend root), generated code left out.</summary>
    public static IEnumerable<string> Under(string folder)
    {
        var path = Path.Combine(Repository.Root, folder);
        return Directory.Exists(path)
            ? Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)
                .Where(file => !Relative(file).Split('/').Any(part => Skipped.Contains(part)))
            : [];
    }

    /// <summary>The path relative to the backend root, with forward slashes.</summary>
    public static string Relative(string path) => Path.GetRelativePath(Repository.Root, path).Replace('\\', '/');
}

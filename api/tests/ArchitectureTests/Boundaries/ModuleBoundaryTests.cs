
namespace ArchitectureTests.Boundaries;

/// <summary>
/// Modules are isolated: a module may use another module only through its <c>Contracts</c> project. This keeps
/// every module ready to move into its own service.
/// </summary>
public sealed class ModuleBoundaryTests
{
    [Fact]
    public void A_Module_ReferencesOtherModules_OnlyThroughTheirContracts()
    {
        var violations =
            from projectFile in Repository.ProjectFiles("src/Modules")
            let project = Path.GetFileNameWithoutExtension(projectFile)
            let module = project.Split('.')[0]
            from reference in Repository.ProjectReferences(projectFile)
            let referencedModule = reference.Split('.')[0]
            where Repository.Modules.Contains(referencedModule) && referencedModule != module
            where !reference.EndsWith(".Contracts", StringComparison.Ordinal)
            select $"{project} -> {reference}";

        Assert.Empty(violations);
    }

    [Fact]
    public void Contracts_AreLeaf_Projects_ThatOtherModulesCanDependOnSafely()
    {
        var violations =
            from projectFile in Repository.ProjectFiles("src/Modules")
            where projectFile.EndsWith(".Contracts.csproj", StringComparison.Ordinal)
            from reference in Repository.ProjectReferences(projectFile)
            select $"{Path.GetFileNameWithoutExtension(projectFile)} -> {reference}";

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_Module_HasAllFiveLayers()
    {
        var missing =
            from module in Repository.Modules
            from layer in Repository.Layers
            let path = Path.Combine(Repository.Root, "src", "Modules", module, $"{module}.{layer}", $"{module}.{layer}.csproj")
            where !File.Exists(path)
            select $"{module}.{layer}";

        Assert.Empty(missing);
    }
}

using System.Reflection;
using Mediator;

namespace ArchitectureTests.Http;

/// <summary>
/// The OpenAPI document names a schema after its .NET type's simple name, and two types with the same name silently
/// become one schema. So the records that travel over HTTP (requests and responses, not commands and queries) and the
/// enums they carry have names no other module uses.
/// </summary>
public sealed class ContractSchemaNameTests
{
    /// <summary>
    /// Enum names some modules already shared before this test covered enums, and the modules that share them. They
    /// stay one schema only while every one of those enums has the same members, so the one schema tells each module
    /// the truth; any other module takes its own name (Notes uses <c>NotePerson</c>).
    /// </summary>
    private static readonly Dictionary<string, string[]> SharedEnumNames = new()
    {
        ["Person"] = ["Expenses", "Shopping", "Todos"],
    };

    [Fact]
    public void Http_Records_HaveNames_NoOtherModuleUses()
    {
        var clashes = Clashes(IsHttpRecord, ["Application", "Api"]);

        Assert.Empty(clashes);
    }

    [Fact]
    public void Enums_HaveNames_NoOtherModuleUses()
    {
        var clashes = Clashes(IsPublicEnum, Repository.Layers)
            .Where(clash => !(SharedEnumNames.TryGetValue(clash.Name, out var shared) && clash.Modules.All(shared.Contains)));

        Assert.Empty(clashes.Select(clash => $"{clash.Name}: {string.Join(", ", clash.Modules)}"));
    }

    [Fact]
    public void Shared_EnumNames_KeepTheSameMembers_InEveryModule()
    {
        var differing = ModuleTypes(IsPublicEnum, Repository.Layers)
            .Where(x => SharedEnumNames.ContainsKey(x.Type.Name))
            .GroupBy(x => x.Type.Name)
            .Where(g => g.Select(x => string.Join(",", Enum.GetNames(x.Type))).Distinct().Count() > 1)
            .Select(g => g.Key);

        Assert.Empty(differing);
    }

    private static IEnumerable<(string Name, IReadOnlyList<string> Modules)> Clashes(
        Func<Type, bool> travels, IEnumerable<string> layers) =>
        ModuleTypes(travels, layers)
            .Select(x => (x.Module, x.Type.Name))
            .Distinct()
            .GroupBy(x => x.Name)
            .Where(g => g.Select(x => x.Module).Distinct().Count() > 1)
            .Select(g => (g.Key, (IReadOnlyList<string>)g.Select(x => x.Module).ToList()));

    private static IEnumerable<(string Module, Type Type)> ModuleTypes(Func<Type, bool> include, IEnumerable<string> layers) =>
        Repository.Modules.SelectMany(module => layers
            .SelectMany(layer => Repository.Assembly($"{module}.{layer}").GetTypes())
            .Where(include)
            .Select(type => (module, type)));

    private static bool IsHttpRecord(Type type) =>
        !type.IsNested
        && !type.IsAbstract
        && type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) is not null
        && !typeof(IMessage).IsAssignableFrom(type);

    private static bool IsPublicEnum(Type type) => type.IsEnum && type.IsPublic;
}

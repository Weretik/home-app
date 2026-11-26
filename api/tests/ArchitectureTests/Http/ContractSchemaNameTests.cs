using System.Reflection;
using Mediator;

namespace ArchitectureTests.Http;

/// <summary>
/// The OpenAPI document names a schema after its .NET type's simple name, and two types with the same name silently
/// become one schema. So the records that travel over HTTP (requests and responses, not commands and queries) have
/// names no other module uses.
/// </summary>
public sealed class ContractSchemaNameTests
{
    [Fact]
    public void Http_Records_HaveNames_NoOtherModuleUses()
    {
        var clashes = Repository.Modules
            .SelectMany(module => new[] { $"{module}.Application", $"{module}.Api" }
                .SelectMany(assembly => Repository.Assembly(assembly).GetTypes())
                .Where(IsHttpRecord)
                .Select(type => (module, type.Name)))
            .Distinct()
            .GroupBy(x => x.Name)
            .Where(g => g.Select(x => x.module).Distinct().Count() > 1)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(x => x.module))}");

        Assert.Empty(clashes);
    }

    private static bool IsHttpRecord(Type type) =>
        !type.IsNested
        && !type.IsAbstract
        && type.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.Instance) is not null
        && !typeof(IMessage).IsAssignableFrom(type);
}

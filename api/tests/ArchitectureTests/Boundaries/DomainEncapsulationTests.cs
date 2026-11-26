using System.Reflection;

namespace ArchitectureTests.Boundaries;

/// <summary>Aggregates change only through their methods, so every business rule has one home.</summary>
public sealed class DomainEncapsulationTests
{
    [Fact]
    public void Domain_Properties_HaveNoPublicSetters()
    {
        var violations =
            from module in Repository.Modules
            from type in Repository.Assembly($"{module}.Domain").GetTypes()
            from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where property.SetMethod is { IsPublic: true } setter && !IsInitOnly(setter)
            select $"{type.Name}.{property.Name}";

        Assert.Empty(violations);
    }

    private static bool IsInitOnly(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers().Any(m => m.Name == "IsExternalInit");
}

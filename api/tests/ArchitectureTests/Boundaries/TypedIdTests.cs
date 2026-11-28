using System.Reflection;
using System.Runtime.CompilerServices;
using BuildingBlocks.Domain.Modeling;

namespace ArchitectureTests.Boundaries;

/// <summary>
/// Every entity has its own id type, and the Domain holds no bare <see cref="Guid"/>: a <c>NoteId</c> cannot be passed
/// where a <c>NoteCategoryId</c> is expected (API rules, «Domain model»).
/// </summary>
public sealed class TypedIdTests
{
    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    [Fact]
    public void Every_Entity_HasItsOwnTypedId()
    {
        var violations =
            from module in Repository.Modules
            from type in Repository.Assembly($"{module}.Domain").GetTypes()
            let id = EntityId(type)
            where id is not null && !IsTypedId(id)
            select $"{module}: {type.Name} : Entity<{id.Name}>";

        Assert.Empty(violations);
    }

    [Fact]
    public void Entities_DoNotShare_AnIdType()
    {
        var shared =
            from module in Repository.Modules
            from type in Repository.Assembly($"{module}.Domain").GetTypes()
            let id = EntityId(type)
            where id is not null
            group type.Name by id into owners
            where owners.Count() > 1
            select $"{owners.Key.Name}: {string.Join(", ", owners)}";

        Assert.Empty(shared);
    }

    [Fact]
    public void The_Domain_HoldsNoBareGuid()
    {
        var violations =
            from module in Repository.Modules
            from type in Repository.Assembly($"{module}.Domain").GetTypes()
            where !IsTypedId(type) && !type.IsDefined(typeof(CompilerGeneratedAttribute), false)
            from member in BareGuidMembers(type)
            select $"{module}: {type.Name}.{member}";

        Assert.Empty(violations);
    }

    /// <summary>The <c>TId</c> of <c>Entity&lt;TId&gt;</c> when <paramref name="type"/> derives from it.</summary>
    private static Type? EntityId(Type type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Entity<>))
                return current.GetGenericArguments()[0];
        }

        return null;
    }

    /// <summary>A <c>readonly record struct …Id(Guid Value)</c>.</summary>
    private static bool IsTypedId(Type type) =>
        type is { IsValueType: true, IsEnum: false }
        && type.Name.EndsWith("Id", StringComparison.Ordinal)
        && type.IsDefined(typeof(IsReadOnlyAttribute), false)
        && type.GetMethod("PrintMembers", BindingFlags.NonPublic | BindingFlags.Instance) is not null
        && type.GetProperty("Value")?.PropertyType == typeof(Guid)
        && type.GetConstructor([typeof(Guid)]) is not null;

    private static IEnumerable<string> BareGuidMembers(Type type)
    {
        foreach (var field in type.GetFields(Declared).Where(f => HoldsGuid(f.FieldType)))
            yield return field.Name;
        foreach (var property in type.GetProperties(Declared).Where(p => HoldsGuid(p.PropertyType)))
            yield return property.Name;
        foreach (var method in type.GetMethods(Declared).Where(m => !m.IsSpecialName))
        {
            if (HoldsGuid(method.ReturnType))
                yield return $"{method.Name} (returns)";
            foreach (var parameter in method.GetParameters().Where(p => HoldsGuid(p.ParameterType)))
                yield return $"{method.Name}({parameter.Name})";
        }

        foreach (var constructor in type.GetConstructors(Declared))
        foreach (var parameter in constructor.GetParameters().Where(p => HoldsGuid(p.ParameterType)))
            yield return $"ctor({parameter.Name})";
    }

    /// <summary>A <see cref="Guid"/>, a nullable one, or a collection or tuple that carries one.</summary>
    private static bool HoldsGuid(Type type) =>
        type == typeof(Guid)
        || (type.HasElementType && HoldsGuid(type.GetElementType()!))
        || (type.IsGenericType && type.GetGenericArguments().Any(HoldsGuid));
}

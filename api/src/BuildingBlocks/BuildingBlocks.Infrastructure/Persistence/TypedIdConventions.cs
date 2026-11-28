using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Maps every typed id of a module (API rules, «Domain model»): a <c>readonly record struct</c> named <c>…Id</c> with
/// one <c>Guid Value</c> and a constructor taking it is stored as that value, wherever it is used (key, reference,
/// nullable reference, query or SQL parameter), so the columns stay plain <c>uuid</c> and no migration is needed.
/// </summary>
public static class TypedIdConventions
{
    public static ModelConfigurationBuilder AddTypedIds(this ModelConfigurationBuilder builder, Assembly domain)
    {
        foreach (var id in TypedIds(domain))
        {
            var converter = typeof(TypedIdConverter<>).MakeGenericType(id);
            builder.Properties(id).HaveConversion(converter);
            // An id passed as a query or raw SQL parameter outside a mapped property is sent as its uuid too.
            builder.DefaultTypeMapping(id).HasConversion(converter);
        }

        return builder;
    }

    /// <summary>The typed ids declared in <paramref name="domain"/>.</summary>
    public static IEnumerable<Type> TypedIds(Assembly domain) => domain.GetTypes()
        .Where(type => type is { IsValueType: true, IsPublic: true } && type.Name.EndsWith("Id", StringComparison.Ordinal)
                       && type.GetProperty("Value")?.PropertyType == typeof(Guid)
                       && type.GetConstructor([typeof(Guid)]) is not null);

    private sealed class TypedIdConverter<TId>() : ValueConverter<TId, Guid>(ToValue(), ToId())
    {
        private static Expression<Func<TId, Guid>> ToValue()
        {
            var id = Expression.Parameter(typeof(TId), "id");
            return Expression.Lambda<Func<TId, Guid>>(Expression.Property(id, "Value"), id);
        }

        private static Expression<Func<Guid, TId>> ToId()
        {
            var value = Expression.Parameter(typeof(Guid), "value");
            return Expression.Lambda<Func<Guid, TId>>(
                Expression.New(typeof(TId).GetConstructor([typeof(Guid)])!, value), value);
        }
    }
}

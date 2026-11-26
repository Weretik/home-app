using System.Reflection;
using BuildingBlocks.Domain.Results;
using FluentValidation;
using Mediator;

namespace ArchitectureTests.Conventions;

/// <summary>
/// Every use case is a command or a query in <c>Features/&lt;Feature&gt;/Commands|Queries/&lt;UseCase&gt;/</c>,
/// returns a <see cref="Result"/>, and has a validator when it carries input.
/// </summary>
public sealed class UseCaseRulesTests
{
    private static readonly Type[] MessageInterfaces = [typeof(ICommand<>), typeof(IQuery<>)];

    private static IEnumerable<Type> Messages() =>
        Repository.Modules
            .SelectMany(module => Repository.Assembly($"{module}.Application").GetTypes())
            .Where(type => !type.IsAbstract && type.GetInterfaces().Any(IsMessageInterface));

    [Fact]
    public void Every_UseCase_ReturnsAResult()
    {
        var violations = Messages()
            .Where(type => !typeof(Result).IsAssignableFrom(ResponseOf(type)))
            .Select(type => type.FullName);

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_UseCase_WithInput_HasAValidator()
    {
        var validated = Repository.Modules
            .SelectMany(module => Repository.Assembly($"{module}.Application").GetTypes())
            .SelectMany(type => type.GetInterfaces())
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
            .Select(i => i.GetGenericArguments()[0])
            .ToHashSet();

        var violations = Messages()
            .Where(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Length > 0)
            .Where(type => !validated.Contains(type))
            .Select(type => type.FullName);

        Assert.Empty(violations);
    }

    [Fact]
    public void Every_UseCase_SitsInItsOwnFolder_UnderCommandsOrQueries()
    {
        var violations = Messages()
            .Where(type =>
            {
                var parts = type.Namespace!.Split('.');
                var kind = type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                    ? "Commands"
                    : "Queries";
                return parts.Length < 5 || parts[^2] != kind || parts[^4] != "Features";
            })
            .Select(type => type.FullName);

        Assert.Empty(violations);
    }

    private static bool IsMessageInterface(Type type) =>
        type.IsGenericType && MessageInterfaces.Contains(type.GetGenericTypeDefinition());

    private static Type ResponseOf(Type message) =>
        message.GetInterfaces().First(IsMessageInterface).GetGenericArguments()[0];
}

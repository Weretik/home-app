using System.Reflection;
using BuildingBlocks.Domain.Results;

namespace BuildingBlocks.Application.Pipeline;

/// <summary>Builds a failed <see cref="Result"/> or <see cref="Result{T}"/> for a handler's response type.</summary>
internal static class FailedResult<TResponse>
{
    /// <summary>Null when the response is not a result; the caller then throws.</summary>
    public static readonly Func<Error, TResponse>? Create = Build();

    private static Func<Error, TResponse>? Build()
    {
        var type = typeof(TResponse);
        if (type == typeof(Result))
            return error => (TResponse)(object)Result.Failure(error);

        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
            return null;

        var failure = type.GetMethod(
            nameof(Result.Failure), BindingFlags.Public | BindingFlags.Static, [typeof(Error)])!;
        return error => (TResponse)failure.Invoke(null, [error])!;
    }
}

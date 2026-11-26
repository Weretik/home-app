namespace BuildingBlocks.Domain.Results;

/// <summary>The outcome of an operation without a value: success, or the <see cref="Error"/> a rule reported.</summary>
public class Result
{
    private static readonly Result Ok = new(null);

    protected Result(Error? error) => Error = error;

    /// <summary>The error of a failure; null on success.</summary>
    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public bool IsFailure => Error is not null;

    public static Result Success() => Ok;

    public static Result Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    /// <summary>Success when the condition holds, otherwise the error.</summary>
    public static Result Ensure(bool condition, Error error) => condition ? Ok : Failure(error);

    /// <summary>The first failure of the checks, in order, or success.</summary>
    public static Result Combine(params Result[] results) => results.FirstOrDefault(r => r.IsFailure) ?? Ok;

    public static implicit operator Result(Error error) => Failure(error);
}

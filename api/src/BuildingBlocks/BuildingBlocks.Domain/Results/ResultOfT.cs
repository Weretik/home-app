namespace BuildingBlocks.Domain.Results;

/// <summary>The outcome of an operation that produces a value on success.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T? value, Error? error)
        : base(error) => _value = value;

    /// <summary>The value of a success; reading it from a failure is a programming error.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"A failed result has no value ({Error!.Code}).");

    public static Result<T> Success(T value) => new(value, null);

    public static new Result<T> Failure(Error error) => new(default, error ?? throw new ArgumentNullException(nameof(error)));

    /// <summary>Applies <paramref name="map"/> to the value of a success; a failure passes through unchanged.</summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> map) =>
        IsSuccess ? Result<TOut>.Success(map(Value)) : Result<TOut>.Failure(Error!);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}

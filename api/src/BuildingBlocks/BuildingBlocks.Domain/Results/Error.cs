namespace BuildingBlocks.Domain.Results;

/// <summary>
/// An expected business outcome that is not a success: a stable code for clients, a message for people, its kind,
/// and the input fields it belongs to. Business rules return it inside a <see cref="Result"/>; exceptions are left
/// for programming errors and failures nobody expects.
/// </summary>
public sealed record Error(
    string Code,
    string Message,
    ErrorType Type = ErrorType.Validation,
    IReadOnlyDictionary<string, string[]>? Fields = null)
{
    public static Error Validation(string code, string message, string? field = null) =>
        new(code, message, ErrorType.Validation, field is null ? null : new Dictionary<string, string[]> { [field] = [message] });

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);
}

namespace BuildingBlocks.Domain.Results;

/// <summary>The kind of a business error. Only the Api layer maps it to an HTTP status.</summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized,
}

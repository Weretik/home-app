using BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Api.Problems;

/// <summary>
/// The one mapping from a business <see cref="Error"/> to an RFC 9457 problem: the status comes from the error
/// type, <c>code</c> is the stable error code, <c>errors</c> holds the field messages of a validation failure.
/// </summary>
public static class ErrorProblems
{
    public static int StatusOf(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError,
    };

    public static ProblemDetails From(Error error, HttpContext httpContext)
    {
        var status = StatusOf(error.Type);
        ProblemDetails problem = error.Fields is { Count: > 0 } fields
            ? new HttpValidationProblemDetails(fields.ToDictionary(f => f.Key, f => f.Value))
            : new ProblemDetails();

        problem.Status = status;
        problem.Title = TitleOf(status);
        problem.Detail = error.Message;
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        return problem;
    }

    private static string TitleOf(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "The request is invalid.",
        StatusCodes.Status401Unauthorized => "Sign in first.",
        StatusCodes.Status403Forbidden => "You do not have access.",
        StatusCodes.Status404NotFound => "Not found.",
        StatusCodes.Status409Conflict => "The request conflicts with the current data.",
        _ => "An unexpected error occurred.",
    };
}

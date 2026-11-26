using BuildingBlocks.Domain.Results;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Api.Problems;

/// <summary>Turns a handler result into an HTTP answer. Endpoints never pick status codes for errors themselves.</summary>
public static class ResultHttpExtensions
{
    public static IResult ToHttpResult(this Result result, HttpContext httpContext) =>
        result.IsSuccess ? TypedResults.NoContent() : Problem(result.Error!, httpContext);

    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext httpContext) =>
        result.IsSuccess ? TypedResults.Ok(result.Value) : Problem(result.Error!, httpContext);

    public static IResult ToCreatedHttpResult<T>(
        this Result<T> result, HttpContext httpContext, Func<T, string> location) =>
        result.IsSuccess
            ? TypedResults.Created(location(result.Value), result.Value)
            : Problem(result.Error!, httpContext);

    private static IResult Problem(Error error, HttpContext httpContext)
    {
        var problem = ErrorProblems.From(error, httpContext);
        return TypedResults.Problem(problem);
    }
}

using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Api.RateLimiting;

public static class RateLimitingRegistration
{
    /// <summary>
    /// The built-in ASP.NET Core rate limiter with the policies in <see cref="RateLimitPolicies"/>. Each client
    /// address gets its own fixed window; a rejected request answers 429 with <c>Retry-After</c> and a problem body.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitOptions>(configuration.GetSection(RateLimitOptions.SectionName));
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(RateLimitPolicies.PublicWrite, http => PerClient(http, limits => limits.PublicWrite));
            options.AddPolicy(RateLimitPolicies.SignIn, http => PerClient(http, limits => limits.SignIn));
            options.OnRejected = WriteProblemAsync;
        });
        return services;
    }

    private static RateLimitPartition<string> PerClient(HttpContext http, Func<RateLimitOptions, FixedWindowLimit> select)
    {
        var limit = select(http.RequestServices.GetRequiredService<IOptions<RateLimitOptions>>().Value);
        return RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit.PermitLimit,
                Window = TimeSpan.FromSeconds(limit.WindowSeconds),
                QueueLimit = 0,
            });
    }

    private static async ValueTask WriteProblemAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var http = context.HttpContext;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            http.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests.",
            Detail = "Too many requests from this address. Wait a little and try again.",
        };
        problem.Extensions["code"] = "too_many_requests";
        problem.Extensions["traceId"] = http.TraceIdentifier;

        await http.RequestServices.GetRequiredService<IProblemDetailsService>()
            .TryWriteAsync(new ProblemDetailsContext { HttpContext = http, ProblemDetails = problem });
    }
}

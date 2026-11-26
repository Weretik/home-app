namespace Host.Api.Composition;

internal static class CorsSetup
{
    public const string PolicyName = "apps";

    /// <summary>The admin app and the site may call the API with the session cookie from the origins in <c>Cors:Origins</c>.</summary>
    public static IServiceCollection AddCorsForApps(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
        return services;
    }
}

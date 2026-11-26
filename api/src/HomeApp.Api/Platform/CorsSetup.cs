namespace HomeApp.Api.Platform;

internal static class CorsSetup
{
    public const string PolicyName = "web";

    /// <summary>The web app calls the API from another origin; allowed origins come from <c>Cors:Origins</c>.</summary>
    public static IServiceCollection AddCorsForWeb(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
        return services;
    }
}

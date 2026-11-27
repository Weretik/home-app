using Identity.Api.Authentication;
using Identity.Api.Endpoints;
using Identity.Application;
using Identity.Contracts;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Api;

/// <summary>
/// The Identity module: Google sign-in for the family, sessions, and who may call the API (ADR 0037). Every endpoint
/// of every module needs a member (or, for reads, the deploy check key) unless it says <c>AllowAnonymous</c> in its own
/// file. The host calls <see cref="AddIdentityModule"/> and <see cref="MapIdentityModule"/>.
/// </summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityApplication();
        services.AddIdentityInfrastructure(configuration);

        // Who sends the request, for the modules that record the author (ADR 0040).
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentMember, CurrentMember>();

        services.Configure<DeployCheckOptions>(configuration.GetSection(DeployCheckOptions.SectionName));
        services.AddAuthentication(IdentitySchemes.Session)
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(IdentitySchemes.Session, null)
            .AddScheme<AuthenticationSchemeOptions, DeployCheckKeyAuthenticationHandler>(IdentitySchemes.DeployCheckKey, null);
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder(IdentitySchemes.Session, IdentitySchemes.DeployCheckKey)
                .RequireAuthenticatedUser()
                .Build());
        return services;
    }

    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/identity")
            .WithTags("Identity")
            .MapSignIn()
            .MapSignOut()
            .MapGetCurrentMember();
        return endpoints;
    }
}

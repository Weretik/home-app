using Identity.Api.Authentication;
using Identity.Api.Endpoints;
using Identity.Application;
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
/// of every module needs a member (or, for reads, the scheduler key) unless it says <c>AllowAnonymous</c> in its own
/// file. The host calls <see cref="AddIdentityModule"/> and <see cref="MapIdentityModule"/>.
/// </summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityApplication();
        services.AddIdentityInfrastructure(configuration);

        services.AddAuthentication(IdentitySchemes.Session)
            .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(IdentitySchemes.Session, null)
            .AddScheme<AuthenticationSchemeOptions, SchedulerKeyAuthenticationHandler>(IdentitySchemes.SchedulerKey, null);
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder(IdentitySchemes.Session, IdentitySchemes.SchedulerKey)
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

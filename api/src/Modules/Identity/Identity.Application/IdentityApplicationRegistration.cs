using FluentValidation;
using Identity.Application.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

/// <summary>Registers what the module's use cases need besides their handlers, which Mediator finds itself.</summary>
public static class IdentityApplicationRegistration
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(IdentityApplicationRegistration).Assembly, includeInternalTypes: true);
        services.AddScoped<SessionAuthenticator>();
        return services;
    }
}

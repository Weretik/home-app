using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Digests.Planning;

namespace Notifications.Application;

/// <summary>Registers what the module's use cases need besides their handlers, which Mediator finds itself.</summary>
public static class NotificationsApplicationRegistration
{
    public static IServiceCollection AddNotificationsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(NotificationsApplicationRegistration).Assembly, includeInternalTypes: true);
        services.AddScoped<DigestInputsReader>();
        return services;
    }
}

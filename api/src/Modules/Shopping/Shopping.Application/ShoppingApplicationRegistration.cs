using FluentValidation;
using Shopping.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Shopping.Application;

/// <summary>Registers what the module's use cases need besides their handlers, which Mediator finds itself.</summary>
public static class ShoppingApplicationRegistration
{
    public static IServiceCollection AddShoppingApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ShoppingApplicationRegistration).Assembly, includeInternalTypes: true);
        // What the Telegram bot reads (Notifications module, through Shopping.Contracts).
        services.AddScoped<IShoppingDigest, ShoppingDigest>();
        return services;
    }
}

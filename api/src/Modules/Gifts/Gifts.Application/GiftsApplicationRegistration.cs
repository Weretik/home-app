using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Gifts.Application;

/// <summary>Registers what the module's use cases need besides their handlers, which Mediator finds itself.</summary>
public static class GiftsApplicationRegistration
{
    public static IServiceCollection AddGiftsApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(GiftsApplicationRegistration).Assembly, includeInternalTypes: true);
        return services;
    }
}

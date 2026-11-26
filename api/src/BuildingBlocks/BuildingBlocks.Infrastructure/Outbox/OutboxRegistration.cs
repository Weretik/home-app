using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox.Dispatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Infrastructure.Outbox;

public static class OutboxRegistration
{
    /// <summary>Registers the outbox table of a module so the tick processes it.</summary>
    public static IServiceCollection AddModuleOutbox<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<OutboxMessageTypes>();
        services.TryAddScoped<IOutboxTick, OutboxTick>();
        services.AddSingleton<IModuleOutbox, ModuleOutbox<TDbContext>>();
        return services;
    }

    /// <summary>Registers a message type under a stable <paramref name="name"/> with its handler.</summary>
    public static IServiceCollection AddOutboxMessage<TMessage, THandler>(this IServiceCollection services, string name)
        where TMessage : class
        where THandler : class, IOutboxMessageHandler<TMessage>
    {
        services.AddScoped<IOutboxMessageHandler<TMessage>, THandler>();
        services.AddSingleton(new OutboxMessageType(
            name,
            typeof(TMessage),
            (provider, message, ct) =>
                provider.GetRequiredService<IOutboxMessageHandler<TMessage>>().HandleAsync((TMessage)message, ct)));
        return services;
    }
}

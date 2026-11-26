using BuildingBlocks.Application.Pipeline;

namespace Host.Api.Composition;

internal static class MediatorSetup
{
    /// <summary>
    /// One pipeline for every command and query: log, then validate, then the handler. The source generator finds
    /// the handlers of each module's Application assembly at compile time.
    /// </summary>
    public static IServiceCollection AddMediatorPipeline(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            // Each module adds its Application assembly here: typeof(<Module>.Application.<Module>ApplicationRegistration).Assembly.
            options.Assemblies = [typeof(Expenses.Application.ExpensesApplicationRegistration).Assembly];
            options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
        });
        return services;
    }
}

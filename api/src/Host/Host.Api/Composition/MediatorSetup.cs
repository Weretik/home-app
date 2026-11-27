using System.Globalization;
using BuildingBlocks.Application.Pipeline;
using FluentValidation;

namespace Host.Api.Composition;

internal static class MediatorSetup
{
    /// <summary>
    /// One pipeline for every command and query: log, then validate, then the handler. The source generator finds
    /// the handlers of each module's Application assembly at compile time.
    /// </summary>
    public static IServiceCollection AddMediatorPipeline(this IServiceCollection services)
    {
        // Validation messages people read are Ukrainian (ADR 0037): FluentValidation's built-in messages too.
        ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("uk");
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            // Each module adds its Application assembly here: typeof(<Module>.Application.<Module>ApplicationRegistration).Assembly.
            options.Assemblies =
            [
                typeof(Identity.Application.IdentityApplicationRegistration).Assembly,
                typeof(Expenses.Application.ExpensesApplicationRegistration).Assembly,
                typeof(Shopping.Application.ShoppingApplicationRegistration).Assembly,
                typeof(Gifts.Application.GiftsApplicationRegistration).Assembly,
                typeof(Todos.Application.TodosApplicationRegistration).Assembly,
                typeof(Notifications.Application.NotificationsApplicationRegistration).Assembly,
            ];
            options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
        });
        return services;
    }
}

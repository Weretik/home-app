using FluentValidation;
using Expenses.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Expenses.Application;

/// <summary>Registers what the module's use cases need besides their handlers, which Mediator finds itself.</summary>
public static class ExpensesApplicationRegistration
{
    public static IServiceCollection AddExpensesApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ExpensesApplicationRegistration).Assembly, includeInternalTypes: true);
        // What the Telegram bot reads (Notifications module, through Expenses.Contracts).
        services.AddScoped<IExpensesDigest, Features.Summary.Digest.ExpensesDigest>();
        return services;
    }
}

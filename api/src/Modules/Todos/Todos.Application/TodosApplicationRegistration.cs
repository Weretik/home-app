using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Todos.Application;

/// <summary>Registers what the module's use cases need besides their handlers, which Mediator finds itself.</summary>
public static class TodosApplicationRegistration
{
    public static IServiceCollection AddTodosApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(TodosApplicationRegistration).Assembly, includeInternalTypes: true);
        return services;
    }
}

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Notes.Application;

/// <summary>Registers what the module's use cases need besides their handlers, which Mediator finds itself.</summary>
public static class NotesApplicationRegistration
{
    public static IServiceCollection AddNotesApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(NotesApplicationRegistration).Assembly, includeInternalTypes: true);
        return services;
    }
}

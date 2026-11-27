using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notes.Application;

namespace Notes.Infrastructure;

public static class NotesInfrastructureRegistration
{
    public static IServiceCollection AddNotesInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<NotesDbContext>(configuration, NotesDbContext.Schema);
        services.AddScoped<INotesDbContext>(provider => provider.GetRequiredService<NotesDbContext>());
        services.AddScoped<IDatabaseInitializer, NotesDatabaseInitializer>();
        return services;
    }
}

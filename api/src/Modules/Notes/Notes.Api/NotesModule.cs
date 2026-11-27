using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notes.Api.Endpoints.Categories;
using Notes.Api.Endpoints.Notes;
using Notes.Application;
using Notes.Infrastructure;

namespace Notes.Api;

/// <summary>
/// The Notes module: what Masha and Vitaliy like, dislike and should know about each other, by category (ADR 0039).
/// The host calls <see cref="AddNotesModule"/> and <see cref="MapNotesModule"/>; nothing else of the module is
/// visible to it.
/// </summary>
public static class NotesModule
{
    public static IServiceCollection AddNotesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddNotesApplication();
        services.AddNotesInfrastructure(configuration);
        return services;
    }

    public static IEndpointRouteBuilder MapNotesModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/notes")
            .WithTags("Notes")
            .MapListNotes()
            .MapCreateNote()
            .MapUpdateNote()
            .MapDeleteNote()
            .MapListNoteCategories()
            .MapCreateNoteCategory()
            .MapUpdateNoteCategory()
            .MapDeleteNoteCategory();
        return endpoints;
    }
}

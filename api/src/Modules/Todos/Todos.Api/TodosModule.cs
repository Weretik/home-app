using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todos.Api.Endpoints.Categories;
using Todos.Api.Endpoints.Steps;
using Todos.Api.Endpoints.Tasks.Editing;
using Todos.Api.Endpoints.Tasks.Reading;
using Todos.Api.Endpoints.Tasks.Status;
using Todos.Application;
using Todos.Infrastructure;

namespace Todos.Api;

/// <summary>
/// The Todos module: the family's to-do list with steps, statuses, priorities and categories (the old «✅ Список
/// дел» sheet). The host calls <see cref="AddTodosModule"/> and <see cref="MapTodosModule"/>; nothing else of the
/// module is visible to it.
/// </summary>
public static class TodosModule
{
    public static IServiceCollection AddTodosModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTodosApplication();
        services.AddTodosInfrastructure(configuration);
        return services;
    }

    public static IEndpointRouteBuilder MapTodosModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/todos")
            .WithTags("Todos")
            .MapListTodoTasks()
            .MapGetTodoTask()
            .MapCreateTodoTask()
            .MapUpdateTodoTask()
            .MapChangeTodoTaskStatus()
            .MapDeleteTodoTask()
            .MapAddTodoTaskStep()
            .MapUpdateTodoTaskStep()
            .MapDeleteTodoTaskStep()
            .MapListTodoCategories()
            .MapCreateTodoCategory()
            .MapUpdateTodoCategory()
            .MapDeleteTodoCategory()
            .MapGetTodoSummary();
        return endpoints;
    }
}

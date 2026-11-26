using BuildingBlocks.Api.Problems;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Todos.Application.Features.Categories.Queries.ListTodoCategories;

namespace Todos.Api.Endpoints.Categories;

internal static class ListTodoCategoriesEndpoint
{
    public static RouteGroupBuilder MapListTodoCategories(this RouteGroupBuilder group)
    {
        group.MapGet("/categories", HandleAsync)
            .WithName("listTodoCategories")
            .WithSummary("Lists the to-do categories in display order, with how many active tasks each holds.")
            .Produces<IReadOnlyList<TodoCategoryItem>>(StatusCodes.Status200OK);
        return group;
    }

    private static async Task<IResult> HandleAsync(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListTodoCategoriesQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}

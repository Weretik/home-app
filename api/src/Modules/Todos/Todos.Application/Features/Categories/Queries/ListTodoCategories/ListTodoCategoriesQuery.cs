using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Categories.Queries.ListTodoCategories;

/// <summary>Every to-do category in display order.</summary>
public sealed record ListTodoCategoriesQuery : IQuery<Result<IReadOnlyList<TodoCategoryItem>>>;

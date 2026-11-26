using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Tasks.Reading;

/// <summary>A task joined with its category, if it has one; the row lists are ordered and projected from.</summary>
public sealed class TaskWithCategory
{
    public required TodoTask Task { get; init; }

    public TodoCategory? Category { get; init; }
}

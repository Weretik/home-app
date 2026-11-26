using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Categories.Commands.UpdateTodoCategory;

/// <summary>Renames a category and changes its emoji, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateTodoCategoryCommand(Guid Id, uint Version, string Name, string? Emoji) : ICommand<Result>;

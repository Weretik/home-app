using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Categories.Commands.DeleteTodoCategory;

/// <summary>
/// Deletes a category, if nobody changed it since <see cref="Version"/>. Its tasks stay, without a category.
/// </summary>
public sealed record DeleteTodoCategoryCommand(Guid Id, uint Version) : ICommand<Result>;

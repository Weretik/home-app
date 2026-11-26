using BuildingBlocks.Domain.Results;
using Mediator;
using Todos.Application.Idempotency;

namespace Todos.Application.Features.Categories.Commands.CreateTodoCategory;

/// <summary>
/// Adds a category at the end of the list. No Idempotency-Key: a repeat hits the unique name and answers 409.
/// </summary>
public sealed record CreateTodoCategoryCommand(string Name, string? Emoji) : ICommand<Result<TodoCreated>>;

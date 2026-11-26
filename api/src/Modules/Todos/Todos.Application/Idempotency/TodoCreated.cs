namespace Todos.Application.Idempotency;

/// <summary>The id of a created task, step or category.</summary>
public sealed record TodoCreated(Guid Id);

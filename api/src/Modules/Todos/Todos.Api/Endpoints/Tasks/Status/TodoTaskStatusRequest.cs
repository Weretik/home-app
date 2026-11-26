using Todos.Domain.Tasks;

namespace Todos.Api.Endpoints.Tasks.Status;

/// <summary>The status a task moves to.</summary>
internal sealed record TodoTaskStatusRequest(TodoStatus Status);

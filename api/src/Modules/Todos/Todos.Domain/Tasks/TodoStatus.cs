namespace Todos.Domain.Tasks;

/// <summary>
/// Where a task stands. <see cref="Todo"/>, <see cref="InProgress"/> and <see cref="Paused"/> are active;
/// <see cref="Done"/> and <see cref="Cancelled"/> are archived.
/// </summary>
public enum TodoStatus
{
    /// <summary>Ждёт.</summary>
    Todo,

    /// <summary>В работе.</summary>
    InProgress,

    /// <summary>Отложено.</summary>
    Paused,

    /// <summary>Сделано.</summary>
    Done,

    /// <summary>Не актуально.</summary>
    Cancelled,
}

namespace Todos.Contracts;

/// <summary>What the Notifications module may read from Todos; implemented by the Todos module.</summary>
public interface ITodosDigest
{
    /// <summary>Every active task (waiting, in progress or paused), by due date (none last), then urgency, then age.</summary>
    Task<IReadOnlyList<OpenTodo>> ListActiveAsync(CancellationToken cancellationToken);
}

namespace Todos.Contracts;

/// <summary>Lets the Telegram bot tick a task done; implemented by the Todos module with the site's rules.</summary>
public interface ITodoCompletion
{
    Task<TodoCompletionOutcome> CompleteAsync(Guid taskId, CancellationToken cancellationToken);
}

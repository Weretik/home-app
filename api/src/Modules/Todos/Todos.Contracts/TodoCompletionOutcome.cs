namespace Todos.Contracts;

/// <summary>What ticking a task done from Telegram did.</summary>
public enum TodoCompletionOutcome
{
    Completed,
    AlreadyClosed,
    NotFound,
}

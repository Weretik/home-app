namespace Todos.Domain.Tasks;

/// <summary>How soon a task matters. Stored as text; lists rank it Urgent first, Low last.</summary>
public enum TodoPriority
{
    /// <summary>Срочно.</summary>
    Urgent,

    /// <summary>Важно.</summary>
    High,

    /// <summary>Обычно, the default.</summary>
    Normal,

    /// <summary>Когда-нибудь.</summary>
    Low,
}

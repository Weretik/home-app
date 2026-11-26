namespace Todos.Application.Features.Tasks.Reading;

/// <summary>One step of a task's checklist.</summary>
public sealed record TaskStepItem(Guid Id, string Title, bool IsDone);

using Todos.Contracts;
using Todos.Domain.Tasks;

namespace Todos.Application.Integrations;

/// <summary>
/// A person moved a task that came from another module into «Сделано»: tell that module, in the same save
/// (spec gifts/002, R-006). Changes made because of such a module's own event never call this, so nothing loops.
/// </summary>
public static class TaskCompletion
{
    public static void PublishIfCompleted(ITodosDbContext db, TodoTask task, TodoStatus statusBefore)
    {
        if (statusBefore != TodoStatus.Done && task.Status == TodoStatus.Done && task is { SourceKind: { } kind, SourceId: { } id })
            db.Publish(new TodoTaskCompleted(task.Id, kind, id));
    }
}

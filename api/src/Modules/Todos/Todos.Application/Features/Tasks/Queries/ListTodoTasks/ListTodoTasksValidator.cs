using BuildingBlocks.Application.Persistence;
using FluentValidation;
using Todos.Application.Features.Tasks.Reading;

namespace Todos.Application.Features.Tasks.Queries.ListTodoTasks;

internal sealed class ListTodoTasksValidator : AbstractValidator<ListTodoTasksQuery>
{
    public ListTodoTasksValidator()
    {
        RuleFor(q => q.View)
            .Must(v => v is TaskViews.ActiveView or TaskViews.ArchiveView)
            .WithMessage("The view is 'active' or 'archive'.");
        RuleForEach(q => q.Assignees).IsInEnum();
        RuleFor(q => q.CategoryId).NotEqual(Guid.Empty);
        RuleFor(q => q)
            .Must(q => !(q.WithoutCategory && q.CategoryId is not null))
            .WithName("categoryId")
            .WithMessage("Send either categoryId or withoutCategory, not both.");
        RuleFor(q => q)
            .Must(q => !(q.WithoutDueDate && (q.DueFrom is not null || q.DueTo is not null)))
            .WithName("withoutDueDate")
            .WithMessage("Send either a due period or withoutDueDate, not both.");
        RuleFor(q => q)
            .Must(q => q.DueFrom is null || q.DueTo is null || q.DueFrom <= q.DueTo)
            .WithName("dueTo")
            .WithMessage("dueTo is not before dueFrom.");
        RuleFor(q => q)
            .Must(q => q.View == TaskViews.ArchiveView || (q.CompletedFrom is null && q.CompletedTo is null))
            .WithName("completedFrom")
            .WithMessage("A completion period applies only to view=archive.");
        RuleFor(q => q)
            .Must(q => q.CompletedFrom is null || q.CompletedTo is null || q.CompletedFrom <= q.CompletedTo)
            .WithName("completedTo")
            .WithMessage("completedTo is not before completedFrom.");
        RuleFor(q => q.Search).ValidSearch();
        RuleFor(q => q.Page).ValidPage();
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

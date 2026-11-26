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
        RuleFor(q => q.Search).ValidSearch();
        RuleFor(q => q.Page).ValidPage();
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

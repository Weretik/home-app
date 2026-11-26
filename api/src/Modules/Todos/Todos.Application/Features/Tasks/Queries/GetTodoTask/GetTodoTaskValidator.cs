using FluentValidation;

namespace Todos.Application.Features.Tasks.Queries.GetTodoTask;

internal sealed class GetTodoTaskValidator : AbstractValidator<GetTodoTaskQuery>
{
    public GetTodoTaskValidator() => RuleFor(q => q.Id).NotEmpty();
}

using FluentValidation;

namespace Todos.Application.Features.Categories.Commands.DeleteTodoCategory;

internal sealed class DeleteTodoCategoryValidator : AbstractValidator<DeleteTodoCategoryCommand>
{
    public DeleteTodoCategoryValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію категорії, яку видаляєте, у заголовку If-Match.");
    }
}

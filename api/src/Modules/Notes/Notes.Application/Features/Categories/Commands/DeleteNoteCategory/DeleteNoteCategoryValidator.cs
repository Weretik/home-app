using FluentValidation;

namespace Notes.Application.Features.Categories.Commands.DeleteNoteCategory;

internal sealed class DeleteNoteCategoryValidator : AbstractValidator<DeleteNoteCategoryCommand>
{
    public DeleteNoteCategoryValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію категорії, яку видаляєте, у заголовку If-Match.");
    }
}

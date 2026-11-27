using FluentValidation;
using Notes.Domain.Categories;

namespace Notes.Application.Features.Categories.Commands.UpdateNoteCategory;

internal sealed class UpdateNoteCategoryValidator : AbstractValidator<UpdateNoteCategoryCommand>
{
    public UpdateNoteCategoryValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненої категорії у заголовку If-Match.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(NoteCategory.MaxNameLength);
        RuleFor(c => c.Emoji).MaximumLength(NoteCategory.MaxEmojiLength);
    }
}

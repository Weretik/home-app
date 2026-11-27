using FluentValidation;
using Notes.Domain.Categories;

namespace Notes.Application.Features.Categories.Commands.CreateNoteCategory;

internal sealed class CreateNoteCategoryValidator : AbstractValidator<CreateNoteCategoryCommand>
{
    public CreateNoteCategoryValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(NoteCategory.MaxNameLength);
        RuleFor(c => c.Emoji).MaximumLength(NoteCategory.MaxEmojiLength);
    }
}

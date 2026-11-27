using FluentValidation;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Commands.UpdateNote;

internal sealed class UpdateNoteValidator : AbstractValidator<UpdateNoteCommand>
{
    public UpdateNoteValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненої нотатки у заголовку If-Match.");
        RuleFor(c => c.About).IsInEnum();
        RuleFor(c => c.Kind).IsInEnum();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(Note.MaxTitleLength);
        RuleFor(c => c.Details).MaximumLength(Note.MaxDetailsLength);
        RuleFor(c => c.Place).MaximumLength(Note.MaxPlaceLength);
        RuleFor(c => c.CategoryId).NotEqual(Guid.Empty);
    }
}

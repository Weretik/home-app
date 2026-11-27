using FluentValidation;
using Notes.Application.Idempotency;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Commands.CreateNote;

internal sealed class CreateNoteValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.About).IsInEnum();
        RuleFor(c => c.Kind).IsInEnum();
        RuleFor(c => c.Title).NotEmpty().MaximumLength(Note.MaxTitleLength);
        RuleFor(c => c.Details).MaximumLength(Note.MaxDetailsLength);
        RuleFor(c => c.Place).MaximumLength(Note.MaxPlaceLength);
        RuleFor(c => c.CategoryId).NotEqual(Guid.Empty);
    }
}

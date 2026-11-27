using FluentValidation;

namespace Notes.Application.Features.Notes.Commands.DeleteNote;

internal sealed class DeleteNoteValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію нотатки, яку видаляєте, у заголовку If-Match.");
    }
}

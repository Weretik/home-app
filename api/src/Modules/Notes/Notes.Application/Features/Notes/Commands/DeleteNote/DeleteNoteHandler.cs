using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Commands.DeleteNote;

public sealed class DeleteNoteHandler(INotesDbContext db) : ICommandHandler<DeleteNoteCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteNoteCommand c, CancellationToken cancellationToken)
    {
        // One conditional DELETE: a stale version deletes nothing.
        var deleted = await db.Notes.Where(x => x.Id == c.Id && x.Version == c.Version).ExecuteDeleteAsync(cancellationToken);
        if (deleted > 0)
            return Result.Success();

        return await db.Notes.AnyAsync(x => x.Id == c.Id, cancellationToken)
            ? NoteErrors.ChangedMeanwhile
            : NoteErrors.NoteNotFound(c.Id);
    }
}

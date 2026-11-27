using BuildingBlocks.Domain.Results;
using Identity.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Commands.UpdateNote;

public sealed class UpdateNoteHandler(INotesDbContext db, TimeProvider clock, ICurrentMember member)
    : ICommandHandler<UpdateNoteCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateNoteCommand c, CancellationToken cancellationToken)
    {
        var note = await db.Notes.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (note is null)
            return NoteErrors.NoteNotFound(c.Id);
        if (note.Version != c.Version)
            return NoteErrors.ChangedMeanwhile;

        if (c.CategoryId is { } categoryId && !await db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken))
            return CategoryErrors.NotFound(categoryId);

        var updated = note.Update(
            c.About, c.Kind, c.Title, c.Details, c.Place, c.IsImportant, c.CategoryId, NoteAuthors.Of(member), clock.GetUtcNow());
        if (updated.IsFailure)
            return updated;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NoteErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

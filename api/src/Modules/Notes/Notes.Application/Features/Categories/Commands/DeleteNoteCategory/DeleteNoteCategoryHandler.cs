using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Categories.Commands.DeleteNoteCategory;

public sealed class DeleteNoteCategoryHandler(INotesDbContext db) : ICommandHandler<DeleteNoteCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteNoteCategoryCommand c, CancellationToken cancellationToken)
    {
        // One conditional DELETE; the database clears category_id of its notes (ON DELETE SET NULL).
        var deleted = await db.Categories.Where(x => x.Id == c.Id && x.Version == c.Version).ExecuteDeleteAsync(cancellationToken);
        if (deleted > 0)
            return Result.Success();

        return await db.Categories.AnyAsync(x => x.Id == c.Id, cancellationToken)
            ? NoteErrors.ChangedMeanwhile
            : CategoryErrors.NotFound(c.Id);
    }
}

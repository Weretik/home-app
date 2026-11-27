using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Categories.Commands.UpdateNoteCategory;

public sealed class UpdateNoteCategoryHandler(INotesDbContext db) : ICommandHandler<UpdateNoteCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateNoteCategoryCommand c, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == new NoteCategoryId(c.Id), cancellationToken);
        if (category is null)
            return CategoryErrors.NotFound(new NoteCategoryId(c.Id));
        if (category.Version != c.Version)
            return NoteErrors.ChangedMeanwhile;

        var name = c.Name.Trim().ToLowerInvariant();
        if (await db.Categories.AnyAsync(x => x.Id != new NoteCategoryId(c.Id) && x.Name.ToLower() == name, cancellationToken))
            return CategoryErrors.NameTaken;

        var renamed = category.Rename(c.Name, c.Emoji);
        if (renamed.IsFailure)
            return renamed;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NoteErrors.ChangedMeanwhile;
        }
        catch (UniqueConstraintViolationException)
        {
            return CategoryErrors.NameTaken;
        }

        return Result.Success();
    }
}

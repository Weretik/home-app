using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notes.Application.Idempotency;
using Notes.Domain.Categories;

namespace Notes.Application.Features.Categories.Commands.CreateNoteCategory;

public sealed class CreateNoteCategoryHandler(INotesDbContext db, TimeProvider clock)
    : ICommandHandler<CreateNoteCategoryCommand, Result<NoteCreated>>
{
    public async ValueTask<Result<NoteCreated>> Handle(CreateNoteCategoryCommand c, CancellationToken cancellationToken)
    {
        var name = c.Name.Trim().ToLowerInvariant();
        if (await db.Categories.AnyAsync(x => x.Name.ToLower() == name, cancellationToken))
            return CategoryErrors.NameTaken;

        var last = await db.Categories.MaxAsync(x => (int?)x.SortOrder, cancellationToken) ?? 0;
        var created = NoteCategory.Create(NoteCategoryId.New(clock.GetUtcNow()), c.Name, c.Emoji, last + 10);
        if (created.IsFailure)
            return created.Error!;

        db.Categories.Add(created.Value);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // Another request took the name between the check and the save.
            return CategoryErrors.NameTaken;
        }

        return new NoteCreated(created.Value.Id.Value);
    }
}

using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Identity.Contracts;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Notes.Application.Idempotency;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Application.Features.Notes.Commands.CreateNote;

public sealed class CreateNoteHandler(INotesDbContext db, TimeProvider clock, ICurrentMember member)
    : ICommandHandler<CreateNoteCommand, Result<NoteCreated>>
{
    private const string Kind = "note";

    public async ValueTask<Result<NoteCreated>> Handle(CreateNoteCommand c, CancellationToken cancellationToken)
    {
        var fingerprint = IdempotentCreates.Fingerprint(new object?[]
        {
            c.About, c.Kind, c.Title, c.Details, c.Place, c.CategoryId, c.IsImportant,
        });
        if (await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken) is { } earlier)
            return earlier;

        if (c.CategoryId is { } categoryId && !await db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken))
            return CategoryErrors.NotFound(categoryId);

        var now = clock.GetUtcNow();
        var created = Note.Create(
            c.About, c.Kind, c.Title, c.Details, c.Place, c.IsImportant, c.CategoryId, NoteAuthors.Of(member), now);
        if (created.IsFailure)
            return created.Error!;

        db.Notes.Add(created.Value);
        db.CreateRequests.Add(new CreateRequest(c.IdempotencyKey, Kind, fingerprint, created.Value.Id, now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            // A racing copy of the same request saved first: answer like a repeat.
            return await IdempotentCreates.EarlierResultAsync(db, c.IdempotencyKey, Kind, fingerprint, cancellationToken)
                ?? IdempotentCreates.KeyReused;
        }

        return new NoteCreated(created.Value.Id);
    }
}

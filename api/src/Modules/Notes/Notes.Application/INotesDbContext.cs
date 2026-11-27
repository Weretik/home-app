using Microsoft.EntityFrameworkCore;
using Notes.Application.Idempotency;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Application;

/// <summary>
/// The module's unit of work: handlers query the sets and call <see cref="SaveChangesAsync"/> once. Infrastructure
/// implements it with the EF Core context that lives in the <c>notes</c> schema.
/// </summary>
public interface INotesDbContext
{
    DbSet<Note> Notes { get; }

    DbSet<NoteCategory> Categories { get; }

    DbSet<CreateRequest> CreateRequests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Notes.Application;
using Notes.Application.Idempotency;
using Notes.Domain.Categories;
using Notes.Domain.Notes;

namespace Notes.Infrastructure;

/// <summary>The module's tables, all in the <c>notes</c> schema, with their own migration history.</summary>
public sealed class NotesDbContext(DbContextOptions<NotesDbContext> options)
    : ModuleDbContext(options, Schema), INotesDbContext
{
    public const string Schema = "notes";

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<NoteCategory> Categories => Set<NoteCategory>();

    public DbSet<CreateRequest> CreateRequests => Set<CreateRequest>();
}

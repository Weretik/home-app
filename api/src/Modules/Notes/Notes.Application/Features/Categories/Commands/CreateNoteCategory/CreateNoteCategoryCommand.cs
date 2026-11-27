using BuildingBlocks.Domain.Results;
using Mediator;
using Notes.Application.Idempotency;

namespace Notes.Application.Features.Categories.Commands.CreateNoteCategory;

/// <summary>
/// Adds a category at the end of the list. No Idempotency-Key: a repeat hits the unique name and answers 409.
/// </summary>
public sealed record CreateNoteCategoryCommand(string Name, string? Emoji) : ICommand<Result<NoteCreated>>;

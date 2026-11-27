using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notes.Application.Features.Notes.Commands.DeleteNote;

/// <summary>Deletes a note for good, if nobody changed it since <see cref="Version"/> (R-009).</summary>
public sealed record DeleteNoteCommand(Guid Id, uint Version) : ICommand<Result>;

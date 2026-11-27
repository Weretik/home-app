using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notes.Application.Features.Categories.Commands.UpdateNoteCategory;

/// <summary>Renames a category and changes its emoji, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateNoteCategoryCommand(Guid Id, uint Version, string Name, string? Emoji) : ICommand<Result>;

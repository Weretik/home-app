using BuildingBlocks.Domain.Results;
using Mediator;

namespace Notes.Application.Features.Categories.Commands.DeleteNoteCategory;

/// <summary>
/// Deletes a category, if nobody changed it since <see cref="Version"/>. Its notes stay, without a category.
/// </summary>
public sealed record DeleteNoteCategoryCommand(Guid Id, uint Version) : ICommand<Result>;

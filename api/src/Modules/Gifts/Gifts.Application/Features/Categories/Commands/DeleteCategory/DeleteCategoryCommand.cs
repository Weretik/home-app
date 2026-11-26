using BuildingBlocks.Domain.Results;
using Mediator;

namespace Gifts.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>Deletes a gift category that no gift uses.</summary>
public sealed record DeleteCategoryCommand(Guid Id) : ICommand<Result>;

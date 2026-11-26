using BuildingBlocks.Domain.Results;
using Mediator;

namespace Gifts.Application.Features.Categories.Commands.RenameCategory;

/// <summary>Gives a gift category a new name (a target value, so a repeat changes nothing); its gifts follow.</summary>
public sealed record RenameCategoryCommand(Guid Id, string Name) : ICommand<Result>;

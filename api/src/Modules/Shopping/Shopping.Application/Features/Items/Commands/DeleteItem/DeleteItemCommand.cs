using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Items.Commands.DeleteItem;

/// <summary>Deletes a list or history row for good, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record DeleteItemCommand(Guid Id, uint Version) : ICommand<Result>;

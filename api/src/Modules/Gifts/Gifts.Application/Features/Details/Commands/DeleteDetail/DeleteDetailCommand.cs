using BuildingBlocks.Domain.Results;
using Mediator;

namespace Gifts.Application.Features.Details.Commands.DeleteDetail;

/// <summary>Deletes a detail for good.</summary>
public sealed record DeleteDetailCommand(Guid Id) : ICommand<Result>;

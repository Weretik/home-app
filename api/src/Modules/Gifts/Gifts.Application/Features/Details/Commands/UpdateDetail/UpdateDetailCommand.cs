using BuildingBlocks.Domain.Results;
using Gifts.Domain.People;
using Mediator;

namespace Gifts.Application.Features.Details.Commands.UpdateDetail;

/// <summary>Replaces a detail's recipient, label and value, if nobody changed it since <see cref="Version"/>; its position stays.</summary>
public sealed record UpdateDetailCommand(Guid Id, uint Version, Recipient Recipient, string Label, string Value) : ICommand<Result>;

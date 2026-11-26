using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Keywords.Commands.DeleteKeyword;

/// <summary>Deletes a keyword, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record DeleteKeywordCommand(Guid Id, uint Version) : ICommand<Result>;

using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Keywords.Commands.UpdateKeyword;

/// <summary>Changes a keyword's text or department, if nobody changed it since <see cref="Version"/>.</summary>
public sealed record UpdateKeywordCommand(Guid Id, uint Version, string Keyword, Guid DepartmentId) : ICommand<Result>;

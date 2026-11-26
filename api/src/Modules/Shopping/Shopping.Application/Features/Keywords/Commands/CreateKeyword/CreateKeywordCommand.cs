using BuildingBlocks.Domain.Results;
using Mediator;
using Shopping.Application.Idempotency;

namespace Shopping.Application.Features.Keywords.Commands.CreateKeyword;

/// <summary>Adds a keyword by hand. A repeat with the same Idempotency-Key and data returns the first id.</summary>
public sealed record CreateKeywordCommand(string IdempotencyKey, string Keyword, Guid DepartmentId) : ICommand<Result<ShoppingCreatedId>>;

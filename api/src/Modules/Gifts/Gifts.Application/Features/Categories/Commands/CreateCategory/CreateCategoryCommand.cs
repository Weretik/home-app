using BuildingBlocks.Domain.Results;
using Gifts.Application.Idempotency;
using Mediator;

namespace Gifts.Application.Features.Categories.Commands.CreateCategory;

/// <summary>Adds a gift category at the end of the list. A repeat with the same Idempotency-Key and name returns the first id.</summary>
public sealed record CreateCategoryCommand(string IdempotencyKey, string Name) : ICommand<Result<CreatedGiftsEntry>>;

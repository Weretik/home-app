using BuildingBlocks.Domain.Results;
using Mediator;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Commands.AddItems;

/// <summary>
/// Adds the items typed as text («молоко, хлеб, яйца 10»). <see cref="DepartmentId"/>, when given, puts all of them
/// into that department instead of matching. A repeat with the same Idempotency-Key and data returns the first result.
/// </summary>
public sealed record AddItemsCommand(string IdempotencyKey, string Text, Person AddedBy, Guid? DepartmentId, bool Urgent)
    : ICommand<Result<AddItemsResult>>;

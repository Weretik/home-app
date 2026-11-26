using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Items.Queries.ListItems;

/// <summary>The list: needed items in store order, then the cart.</summary>
public sealed record ListItemsQuery : IQuery<Result<ShoppingList>>;

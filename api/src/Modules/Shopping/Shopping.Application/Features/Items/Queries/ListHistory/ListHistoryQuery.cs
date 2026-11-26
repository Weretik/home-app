using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Items.Queries.ListHistory;

/// <summary>Bought items that left the list, newest bought first, one page.</summary>
public sealed record ListHistoryQuery(int Page, int PageSize) : IQuery<Result<ShoppingHistoryPage>>;

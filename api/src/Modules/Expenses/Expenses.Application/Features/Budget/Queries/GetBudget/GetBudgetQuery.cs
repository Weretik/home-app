using BuildingBlocks.Domain.Results;
using Mediator;

namespace Expenses.Application.Features.Budget.Queries.GetBudget;

/// <summary>The budget settings and every spending limit, in their order: what the «Настройки» page shows.</summary>
public sealed record GetBudgetQuery : IQuery<Result<BudgetView>>;

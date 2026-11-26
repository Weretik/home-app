using BuildingBlocks.Domain.Results;
using Mediator;

namespace Todos.Application.Features.Summary.Queries.GetTodoSummary;

/// <summary>The to-do block of the home page, for today in Kyiv.</summary>
public sealed record GetTodoSummaryQuery : IQuery<Result<TodoSummary>>;

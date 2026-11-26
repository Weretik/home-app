namespace BuildingBlocks.Application.Persistence;

/// <summary>One page of a list, counted and cut in SQL.</summary>
public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public const int MaxPageSize = 100;
}

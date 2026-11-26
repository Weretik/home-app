using FluentValidation;

namespace BuildingBlocks.Application.Persistence;

public static class PagingRules
{
    public static IRuleBuilderOptions<T, int> ValidPage<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThanOrEqualTo(1);

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(1, PagedList<object>.MaxPageSize);

    public static IRuleBuilderOptions<T, string?> ValidSearch<T>(this IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(100);
}

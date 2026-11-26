using FluentValidation;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Departments.Queries.MatchDepartment;

internal sealed class MatchDepartmentValidator : AbstractValidator<MatchDepartmentQuery>
{
    public MatchDepartmentValidator() => RuleFor(q => q.Name).NotEmpty().MaximumLength(ShoppingItem.MaxNameLength);
}

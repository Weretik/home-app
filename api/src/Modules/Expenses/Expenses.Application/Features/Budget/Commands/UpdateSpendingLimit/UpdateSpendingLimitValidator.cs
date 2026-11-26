using Expenses.Domain.Budget;
using FluentValidation;

namespace Expenses.Application.Features.Budget.Commands.UpdateSpendingLimit;

internal sealed class UpdateSpendingLimitValidator : AbstractValidator<UpdateSpendingLimitCommand>
{
    public UpdateSpendingLimitValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the limit you edited in the If-Match header.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(SpendingLimit.MaxNameLength);
        RuleFor(c => c.Period).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.CategoryIds).NotEmpty().Must(ids => ids.Count <= 100);
    }
}

using Expenses.Application.Idempotency;
using Expenses.Domain.Budget;
using FluentValidation;

namespace Expenses.Application.Features.Budget.Commands.CreateSpendingLimit;

internal sealed class CreateSpendingLimitValidator : AbstractValidator<CreateSpendingLimitCommand>
{
    public CreateSpendingLimitValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(SpendingLimit.MaxNameLength);
        RuleFor(c => c.Period).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.CategoryIds).NotEmpty().Must(ids => ids.Count <= 100);
    }
}

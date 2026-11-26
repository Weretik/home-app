using FluentValidation;

namespace Expenses.Application.Features.Budget.Commands.DeleteSpendingLimit;

internal sealed class DeleteSpendingLimitValidator : AbstractValidator<DeleteSpendingLimitCommand>
{
    public DeleteSpendingLimitValidator() => RuleFor(c => c.Id).NotEmpty();
}

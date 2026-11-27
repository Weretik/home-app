using Expenses.Domain.Budget;
using Expenses.Domain.Budget.Amounts;
using FluentValidation;

namespace Expenses.Application.Features.Budget.Commands.UpdateSpendingLimit;

internal sealed class UpdateSpendingLimitValidator : AbstractValidator<UpdateSpendingLimitCommand>
{
    public UpdateSpendingLimitValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненого ліміту у заголовку If-Match.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(SpendingLimit.MaxNameLength);
        RuleFor(c => c.Period).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.CategoryIds).NotEmpty().Must(ids => ids.Count <= 100);
        RuleFor(c => c.AmountFrom)
            .Must(d => d is null || AmountHistory.Covers(d.Value))
            .WithMessage("Місяць, з якого діє сума, має бути між 2000-01-01 і 2100-12-31.");
    }
}

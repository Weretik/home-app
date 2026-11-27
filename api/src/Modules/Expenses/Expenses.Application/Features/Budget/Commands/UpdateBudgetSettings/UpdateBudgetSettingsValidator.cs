using Expenses.Domain.Budget.Amounts;
using FluentValidation;

namespace Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;

internal sealed class UpdateBudgetSettingsValidator : AbstractValidator<UpdateBudgetSettingsCommand>
{
    public UpdateBudgetSettingsValidator()
    {
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію змінених налаштувань у заголовку If-Match.");
        RuleFor(c => c.MonthlyLimit).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SavingsGoal).GreaterThanOrEqualTo(0);
        RuleFor(c => c.LimitsFrom)
            .Must(d => d is null || d.Value.Year is >= 2000 and <= 2100)
            .WithMessage("Дата має бути в межах 2000–2100 років.");
        RuleFor(c => c.MonthlyLimitFrom)
            .Must(d => d is null || AmountHistory.Covers(d.Value))
            .WithMessage("Місяць, з якого діє загальний ліміт, має бути між 2000-01-01 і 2100-12-31.");
    }
}

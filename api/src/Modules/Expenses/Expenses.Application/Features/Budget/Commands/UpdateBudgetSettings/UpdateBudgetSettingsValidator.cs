using FluentValidation;

namespace Expenses.Application.Features.Budget.Commands.UpdateBudgetSettings;

internal sealed class UpdateBudgetSettingsValidator : AbstractValidator<UpdateBudgetSettingsCommand>
{
    public UpdateBudgetSettingsValidator()
    {
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the settings you edited in the If-Match header.");
        RuleFor(c => c.MonthlyLimit).GreaterThanOrEqualTo(0);
        RuleFor(c => c.SavingsGoal).GreaterThanOrEqualTo(0);
    }
}

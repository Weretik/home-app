using FluentValidation;

namespace Expenses.Application.Features.Planned.Commands.DeletePlannedExpense;

internal sealed class DeletePlannedExpenseValidator : AbstractValidator<DeletePlannedExpenseCommand>
{
    public DeletePlannedExpenseValidator() => RuleFor(c => c.Id).NotEmpty();
}

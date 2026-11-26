using FluentValidation;

namespace Expenses.Application.Features.Expenses.Commands.DeleteExpense;

internal sealed class DeleteExpenseValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseValidator() => RuleFor(c => c.Id).NotEmpty();
}

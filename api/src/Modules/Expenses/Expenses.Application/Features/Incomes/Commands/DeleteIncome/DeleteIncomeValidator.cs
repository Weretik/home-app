using FluentValidation;

namespace Expenses.Application.Features.Incomes.Commands.DeleteIncome;

internal sealed class DeleteIncomeValidator : AbstractValidator<DeleteIncomeCommand>
{
    public DeleteIncomeValidator() => RuleFor(c => c.Id).NotEmpty();
}

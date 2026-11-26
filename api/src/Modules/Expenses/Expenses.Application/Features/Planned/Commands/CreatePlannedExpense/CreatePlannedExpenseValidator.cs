using Expenses.Application.Idempotency;
using Expenses.Domain.Planning;
using FluentValidation;

namespace Expenses.Application.Features.Planned.Commands.CreatePlannedExpense;

internal sealed class CreatePlannedExpenseValidator : AbstractValidator<CreatePlannedExpenseCommand>
{
    public CreatePlannedExpenseValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Year).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Month).InclusiveBetween(1, 12);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(PlannedExpense.MaxNameLength);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Payer).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
    }
}

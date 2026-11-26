using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Incomes.Commands.CreateIncome;

internal sealed class CreateIncomeValidator : AbstractValidator<CreateIncomeCommand>
{
    public CreateIncomeValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Person).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

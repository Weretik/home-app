using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Expenses.Commands.CreateExpense;

internal sealed class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Payer).IsInEnum();
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.PaymentMethod).IsInEnum();
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Expenses.Commands.UpdateExpense;

internal sealed class UpdateExpenseValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the entry you edited in the If-Match header.");
        RuleFor(c => c.Payer).IsInEnum();
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.PaymentMethod).IsInEnum();
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

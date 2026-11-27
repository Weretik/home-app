using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Planned.Commands.RecordPlannedExpense;

internal sealed class RecordPlannedExpenseValidator : AbstractValidator<RecordPlannedExpenseCommand>
{
    public RecordPlannedExpenseValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію плану, який записуєте, у заголовку If-Match.");
        RuleFor(c => c.PaymentMethod).IsInEnum();
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

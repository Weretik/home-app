using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Incomes.Commands.UpdateIncome;

internal sealed class UpdateIncomeValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненого запису у заголовку If-Match.");
        RuleFor(c => c.Person).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

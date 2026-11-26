using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Incomes.Commands.UpdateIncome;

internal sealed class UpdateIncomeValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the entry you edited in the If-Match header.");
        RuleFor(c => c.Person).IsInEnum();
        RuleFor(c => c.Amount).GreaterThan(0);
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

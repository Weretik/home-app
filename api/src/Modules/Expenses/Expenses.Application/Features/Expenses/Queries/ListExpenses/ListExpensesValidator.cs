using BuildingBlocks.Application.Persistence;
using FluentValidation;

namespace Expenses.Application.Features.Expenses.Queries.ListExpenses;

internal sealed class ListExpensesValidator : AbstractValidator<ListExpensesQuery>
{
    public const int MaxPeriodDays = 366;

    public ListExpensesValidator()
    {
        RuleFor(q => q)
            .Must(q => (q.Year, q.Month, q.From, q.To) is (not null, not null, null, null) or (null, null, not null, not null))
            .WithName("period")
            .WithMessage("Надішліть або рік і місяць, або from і to.");

        When(q => q.Year is not null || q.Month is not null, () =>
        {
            RuleFor(q => q.Year).NotNull().InclusiveBetween(2000, 2100);
            RuleFor(q => q.Month).NotNull().InclusiveBetween(1, 12);
        });

        When(q => q.From is not null && q.To is not null, () =>
        {
            RuleFor(q => q.From).Must(d => d!.Value.Year is >= 2000 and <= 2100).WithMessage("Дата має бути в межах 2000–2100 років.");
            RuleFor(q => q.To)
                .Must((q, to) => to >= q.From).WithMessage("Період закінчується раніше, ніж починається.")
                .Must((q, to) => to!.Value.DayNumber - q.From!.Value.DayNumber < MaxPeriodDays)
                .WithMessage($"Період — не більше {MaxPeriodDays} днів.");
        });

        RuleFor(q => q.Payer).IsInEnum();
        RuleFor(q => q.Page).ValidPage();
        RuleFor(q => q.PageSize).ValidPageSize();
    }
}

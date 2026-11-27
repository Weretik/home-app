using FluentValidation;

namespace Shopping.Application.Features.Departments.Commands.ReorderDepartments;

internal sealed class ReorderDepartmentsValidator : AbstractValidator<ReorderDepartmentsCommand>
{
    public const int MaxDepartments = 100;

    public ReorderDepartmentsValidator()
    {
        RuleFor(c => c.DepartmentIds)
            .NotEmpty()
            .Must(ids => ids.Count <= MaxDepartments).WithMessage($"Не більше {MaxDepartments} відділів.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Кожен відділ має бути в списку один раз.")
            .Must(ids => !ids.Contains(Guid.Empty)).WithMessage("Ідентифікатор відділу порожній.");
    }
}

using FluentValidation;

namespace Shopping.Application.Features.Departments.Commands.ReorderDepartments;

internal sealed class ReorderDepartmentsValidator : AbstractValidator<ReorderDepartmentsCommand>
{
    public const int MaxDepartments = 100;

    public ReorderDepartmentsValidator()
    {
        RuleFor(c => c.DepartmentIds)
            .NotEmpty()
            .Must(ids => ids.Count <= MaxDepartments).WithMessage($"At most {MaxDepartments} departments.")
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Each department is sent once.")
            .Must(ids => !ids.Contains(Guid.Empty)).WithMessage("A department id is empty.");
    }
}

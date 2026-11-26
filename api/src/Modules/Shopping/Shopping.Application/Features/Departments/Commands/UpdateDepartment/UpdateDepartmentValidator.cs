using FluentValidation;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Departments.Commands.UpdateDepartment;

internal sealed class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the department in the If-Match header.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Department.MaxNameLength);
        RuleFor(c => c.Icon).MaximumLength(Department.MaxIconLength);
    }
}

using FluentValidation;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Departments.Commands.UpdateDepartment;

internal sealed class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію відділу у заголовку If-Match.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Department.MaxNameLength);
        RuleFor(c => c.Icon).MaximumLength(Department.MaxIconLength);
    }
}

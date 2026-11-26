using FluentValidation;

namespace Shopping.Application.Features.Departments.Commands.DeleteDepartment;

internal sealed class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the department in the If-Match header.");
    }
}

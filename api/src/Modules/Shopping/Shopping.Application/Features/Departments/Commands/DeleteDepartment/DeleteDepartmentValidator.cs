using FluentValidation;

namespace Shopping.Application.Features.Departments.Commands.DeleteDepartment;

internal sealed class DeleteDepartmentValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію відділу у заголовку If-Match.");
    }
}

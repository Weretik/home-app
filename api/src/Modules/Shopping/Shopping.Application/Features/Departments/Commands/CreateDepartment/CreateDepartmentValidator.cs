using FluentValidation;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Departments.Commands.CreateDepartment;

internal sealed class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Department.MaxNameLength);
        RuleFor(c => c.Icon).MaximumLength(Department.MaxIconLength);
    }
}

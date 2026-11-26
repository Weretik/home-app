using FluentValidation;
using Shopping.Application.Idempotency;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Keywords.Commands.CreateKeyword;

internal sealed class CreateKeywordValidator : AbstractValidator<CreateKeywordCommand>
{
    public CreateKeywordValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Keyword).NotEmpty().MaximumLength(DepartmentKeyword.MaxInputLength);
        RuleFor(c => c.DepartmentId).NotEmpty();
    }
}

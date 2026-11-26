using FluentValidation;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Keywords.Commands.UpdateKeyword;

internal sealed class UpdateKeywordValidator : AbstractValidator<UpdateKeywordCommand>
{
    public UpdateKeywordValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the keyword in the If-Match header.");
        RuleFor(c => c.Keyword).NotEmpty().MaximumLength(DepartmentKeyword.MaxInputLength);
        RuleFor(c => c.DepartmentId).NotEmpty();
    }
}

using FluentValidation;
using Shopping.Domain.Departments;

namespace Shopping.Application.Features.Keywords.Commands.UpdateKeyword;

internal sealed class UpdateKeywordValidator : AbstractValidator<UpdateKeywordCommand>
{
    public UpdateKeywordValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію ключового слова у заголовку If-Match.");
        RuleFor(c => c.Keyword).NotEmpty().MaximumLength(DepartmentKeyword.MaxInputLength);
        RuleFor(c => c.DepartmentId).NotEmpty();
    }
}

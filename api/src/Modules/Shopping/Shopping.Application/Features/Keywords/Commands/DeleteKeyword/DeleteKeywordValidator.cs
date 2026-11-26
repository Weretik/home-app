using FluentValidation;

namespace Shopping.Application.Features.Keywords.Commands.DeleteKeyword;

internal sealed class DeleteKeywordValidator : AbstractValidator<DeleteKeywordCommand>
{
    public DeleteKeywordValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the keyword in the If-Match header.");
    }
}

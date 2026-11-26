using FluentValidation;

namespace Gifts.Application.Features.Details.Commands.DeleteDetail;

internal sealed class DeleteDetailValidator : AbstractValidator<DeleteDetailCommand>
{
    public DeleteDetailValidator() => RuleFor(c => c.Id).NotEmpty();
}

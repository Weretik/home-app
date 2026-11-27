using FluentValidation;
using Gifts.Domain.People;

namespace Gifts.Application.Features.Details.Commands.UpdateDetail;

internal sealed class UpdateDetailValidator : AbstractValidator<UpdateDetailCommand>
{
    public UpdateDetailValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненого нюансу у заголовку If-Match.");
        RuleFor(c => c.Recipient).IsInEnum();
        RuleFor(c => c.Label).NotEmpty().MaximumLength(Detail.MaxLabelLength);
        RuleFor(c => c.Value).NotEmpty().MaximumLength(Detail.MaxValueLength);
    }
}

using FluentValidation;
using Gifts.Application.Idempotency;
using Gifts.Domain.People;

namespace Gifts.Application.Features.Details.Commands.CreateDetail;

internal sealed class CreateDetailValidator : AbstractValidator<CreateDetailCommand>
{
    public CreateDetailValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Recipient).IsInEnum();
        RuleFor(c => c.Label).NotEmpty().MaximumLength(Detail.MaxLabelLength);
        RuleFor(c => c.Value).NotEmpty().MaximumLength(Detail.MaxValueLength);
    }
}

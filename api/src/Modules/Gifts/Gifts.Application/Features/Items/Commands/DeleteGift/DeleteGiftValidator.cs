using FluentValidation;

namespace Gifts.Application.Features.Items.Commands.DeleteGift;

internal sealed class DeleteGiftValidator : AbstractValidator<DeleteGiftCommand>
{
    public DeleteGiftValidator() => RuleFor(c => c.Id).NotEmpty();
}

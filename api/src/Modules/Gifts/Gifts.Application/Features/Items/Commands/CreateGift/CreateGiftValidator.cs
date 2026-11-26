using FluentValidation;
using Gifts.Application.Idempotency;
using Gifts.Domain.Wishlist;

namespace Gifts.Application.Features.Items.Commands.CreateGift;

internal sealed class CreateGiftValidator : AbstractValidator<CreateGiftCommand>
{
    public CreateGiftValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Recipient).IsInEnum();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Gift.MaxNameLength);
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Priority).IsInEnum();
        RuleFor(c => c.Price).GreaterThan(0).When(c => c.Price is not null);
        RuleFor(c => c.Link).MaximumLength(Gift.MaxLinkLength);
        RuleFor(c => c.Occasion).MaximumLength(Gift.MaxOccasionLength);
        RuleFor(c => c.Notes).MaximumLength(Gift.MaxNotesLength);
        RuleFor(c => c.Giver).IsInEnum().When(c => c.Giver is not null);
        RuleFor(c => c.Variants).NotNull().Must(v => v.Count <= Gift.MaxVariants)
            .WithMessage($"A gift has at most {Gift.MaxVariants} variants.");
        RuleForEach(c => c.Variants).NotNull().SetValidator(new VariantDraftValidator());
    }
}

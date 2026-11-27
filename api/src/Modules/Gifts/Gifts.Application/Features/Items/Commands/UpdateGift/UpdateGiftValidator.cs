using FluentValidation;
using Gifts.Domain.Wishlist;

namespace Gifts.Application.Features.Items.Commands.UpdateGift;

internal sealed class UpdateGiftValidator : AbstractValidator<UpdateGiftCommand>
{
    public UpdateGiftValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію зміненого подарунка у заголовку If-Match.");
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
            .WithMessage($"У подарунка не більше {Gift.MaxVariants} варіантів.");
        RuleForEach(c => c.Variants).NotNull().SetValidator(new VariantDraftValidator());
    }
}

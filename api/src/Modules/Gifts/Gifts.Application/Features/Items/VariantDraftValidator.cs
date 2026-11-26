using FluentValidation;
using Gifts.Domain.Wishlist;

namespace Gifts.Application.Features.Items;

/// <summary>The shape of one variant, shared by the gift's create and update; the domain checks the link itself.</summary>
internal sealed class VariantDraftValidator : AbstractValidator<VariantDraft>
{
    public VariantDraftValidator()
    {
        RuleFor(v => v.Link).NotEmpty().MaximumLength(Gift.MaxLinkLength);
        RuleFor(v => v.Title).MaximumLength(Gift.MaxNameLength);
        RuleFor(v => v.Price).GreaterThan(0).When(v => v.Price is not null);
    }
}

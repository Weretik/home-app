using FluentValidation;

namespace Gifts.Application.Features.Items.Queries.ListGifts;

/// <summary>A flag has no wrong value; the validator exists so every query with input has one.</summary>
internal sealed class ListGiftsValidator : AbstractValidator<ListGiftsQuery>
{
    public ListGiftsValidator() => RuleFor(q => q.Archived).NotNull();
}

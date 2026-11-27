using BuildingBlocks.Application.Persistence;
using FluentValidation;

namespace Notes.Application.Features.Notes.Queries.ListNotes;

internal sealed class ListNotesValidator : AbstractValidator<ListNotesQuery>
{
    public ListNotesValidator()
    {
        RuleFor(q => q.About).IsInEnum();
        RuleForEach(q => q.Kinds).IsInEnum();
        RuleFor(q => q.CategoryId).NotEqual(Guid.Empty);
        RuleFor(q => q)
            .Must(q => !(q.WithoutCategory && q.CategoryId is not null))
            .WithName("categoryId")
            .WithMessage("Надішліть або categoryId, або withoutCategory, але не обидва.");
        RuleFor(q => q.Search).ValidSearch();
    }
}

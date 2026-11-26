using BuildingBlocks.Domain.Results;
using Gifts.Domain.Categories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Categories.Commands.DeleteCategory;

/// <summary>
/// Refuses while a gift uses the category. The foreign key (<c>restrict</c>) guards the race with a gift added to it
/// meanwhile: the delete then fails in the database and answers the same conflict.
/// </summary>
public sealed class DeleteCategoryHandler(IGiftsDbContext db) : ICommandHandler<DeleteCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteCategoryCommand c, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == c.Id, cancellationToken);
        if (category is null)
            return CategoryErrors.NotFound(c.Id);
        if (await db.Gifts.AnyAsync(g => g.CategoryId == c.Id, cancellationToken))
            return CategoryErrors.InUse;

        db.Categories.Remove(category);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CategoryErrors.NotFound(c.Id);
        }
        catch (DbUpdateException)
        {
            // The only other way a single-row delete fails is the foreign key of a gift added meanwhile.
            return CategoryErrors.InUse;
        }

        return Result.Success();
    }
}

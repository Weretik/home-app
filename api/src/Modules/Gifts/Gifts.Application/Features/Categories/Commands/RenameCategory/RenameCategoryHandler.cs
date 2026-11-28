using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Gifts.Domain.Categories;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Categories.Commands.RenameCategory;

public sealed class RenameCategoryHandler(IGiftsDbContext db) : ICommandHandler<RenameCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(RenameCategoryCommand c, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == new CategoryId(c.Id), cancellationToken);
        if (category is null)
            return CategoryErrors.NotFound(new CategoryId(c.Id));

        var renamed = category.Rename(c.Name);
        if (renamed.IsFailure)
            return renamed;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintViolationException)
        {
            return CategoryErrors.NameTaken;
        }

        return Result.Success();
    }
}

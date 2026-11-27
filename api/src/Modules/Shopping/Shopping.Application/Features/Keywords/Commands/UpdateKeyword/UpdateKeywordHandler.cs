using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;
using Shopping.Domain.Parsing;

namespace Shopping.Application.Features.Keywords.Commands.UpdateKeyword;

public sealed class UpdateKeywordHandler(IShoppingDbContext db) : ICommandHandler<UpdateKeywordCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateKeywordCommand c, CancellationToken cancellationToken)
    {
        var keyword = await db.Keywords.FirstOrDefaultAsync(k => k.Id == new DepartmentKeywordId(c.Id), cancellationToken);
        if (keyword is null)
            return DepartmentErrors.KeywordNotFound(new DepartmentId(c.Id));
        if (keyword.Version != c.Version)
            return ShoppingItemErrors.ChangedMeanwhile;
        if (!await db.Departments.AnyAsync(d => d.Id == new DepartmentId(c.DepartmentId), cancellationToken))
            return DepartmentErrors.NotFound(new DepartmentId(c.DepartmentId));

        var text = NameNormalizer.Normalize(c.Keyword);
        if (await db.Keywords.AnyAsync(k => k.Keyword == text && k.Id != new DepartmentKeywordId(c.Id), cancellationToken))
            return DepartmentErrors.KeywordTaken;

        var updated = keyword.Update(c.Keyword, new DepartmentId(c.DepartmentId));
        if (updated.IsFailure)
            return updated;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ShoppingItemErrors.ChangedMeanwhile;
        }
        catch (UniqueConstraintViolationException)
        {
            return DepartmentErrors.KeywordTaken;
        }

        return Result.Success();
    }
}

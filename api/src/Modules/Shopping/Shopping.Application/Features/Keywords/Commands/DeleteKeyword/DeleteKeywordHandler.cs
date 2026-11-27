using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Keywords.Commands.DeleteKeyword;

public sealed class DeleteKeywordHandler(IShoppingDbContext db) : ICommandHandler<DeleteKeywordCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteKeywordCommand c, CancellationToken cancellationToken)
    {
        var keyword = await db.Keywords.FirstOrDefaultAsync(k => k.Id == new DepartmentKeywordId(c.Id), cancellationToken);
        if (keyword is null)
            return DepartmentErrors.KeywordNotFound(new DepartmentId(c.Id));
        if (keyword.Version != c.Version)
            return ShoppingItemErrors.ChangedMeanwhile;

        db.Keywords.Remove(keyword);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ShoppingItemErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

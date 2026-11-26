using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Application.Features.Dictionary;
using Shopping.Application.Features.Items.Views;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Items.Commands.UpdateItem;

public sealed class UpdateItemHandler(IShoppingDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateItemCommand, Result<ShoppingItemView>>
{
    public async ValueTask<Result<ShoppingItemView>> Handle(UpdateItemCommand c, CancellationToken cancellationToken)
    {
        var window = await ItemWindow.ReadAsync(db, clock, cancellationToken);
        var item = await db.Items.FirstOrDefaultAsync(i => i.Id == c.Id, cancellationToken);
        if (item is null || !CartWindow.IsInList(item, window.Cutoff))
            return ShoppingItemErrors.NotFound(c.Id);
        if (item.Version != c.Version)
            return ShoppingItemErrors.ChangedMeanwhile;

        var department = await db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == c.DepartmentId, cancellationToken);
        if (department is null)
            return DepartmentErrors.NotFound(c.DepartmentId);

        var moved = item.DepartmentId != department.Id;
        var updated = item.Update(c.Name, c.Quantity, c.Note, department.Id, c.Urgent);
        if (updated.IsFailure)
            return updated.Error!;
        if (moved && !department.IsFallback)
            await KeywordLearning.LearnAsync(db, item.NormalizedName, department.Id, window.Now, cancellationToken);

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
            // Someone added the same keyword by hand at this moment.
            return DepartmentErrors.KeywordTaken;
        }

        return ItemViews.ToView(item, department);
    }
}

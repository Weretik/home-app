using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Departments.Commands.DeleteDepartment;

/// <summary>
/// The one command with an explicit transaction: the items (possibly thousands of history rows) and keywords move
/// with set-based updates, then the department row goes, all or nothing. The foreign keys are restrictive, so a row
/// that arrived meanwhile makes the delete fail instead of losing its department.
/// </summary>
public sealed class DeleteDepartmentHandler(IShoppingDbContext db) : ICommandHandler<DeleteDepartmentCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteDepartmentCommand c, CancellationToken cancellationToken)
    {
        var department = await db.Departments.FirstOrDefaultAsync(d => d.Id == c.Id, cancellationToken);
        if (department is null)
            return DepartmentErrors.NotFound(c.Id);
        if (department.Version != c.Version)
            return ShoppingItemErrors.ChangedMeanwhile;
        var deletable = department.EnsureDeletable();
        if (deletable.IsFailure)
            return deletable;

        var fallbackId = await db.Departments.Where(d => d.IsFallback).Select(d => d.Id).SingleAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.Items.Where(i => i.DepartmentId == department.Id)
            .ExecuteUpdateAsync(set => set.SetProperty(i => i.DepartmentId, fallbackId), cancellationToken);
        await db.Keywords.Where(k => k.DepartmentId == department.Id)
            .ExecuteUpdateAsync(set => set.SetProperty(k => k.DepartmentId, fallbackId), cancellationToken);
        db.Departments.Remove(department);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Changed meanwhile (version) or a row added to it between the moves and the delete (foreign key).
            return ShoppingItemErrors.ChangedMeanwhile;
        }

        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

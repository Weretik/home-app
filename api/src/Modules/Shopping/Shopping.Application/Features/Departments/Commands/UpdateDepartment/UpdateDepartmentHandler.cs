using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Departments.Commands.UpdateDepartment;

public sealed class UpdateDepartmentHandler(IShoppingDbContext db) : ICommandHandler<UpdateDepartmentCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateDepartmentCommand c, CancellationToken cancellationToken)
    {
        var department = await db.Departments.FirstOrDefaultAsync(d => d.Id == new DepartmentId(c.Id), cancellationToken);
        if (department is null)
            return DepartmentErrors.NotFound(new DepartmentId(c.Id));
        if (department.Version != c.Version)
            return ShoppingItemErrors.ChangedMeanwhile;

        var name = c.Name.Trim();
        if (await db.Departments.AnyAsync(d => d.Name == name && d.Id != new DepartmentId(c.Id), cancellationToken))
            return DepartmentErrors.NameTaken;

        var renamed = department.Rename(name, c.Icon);
        if (renamed.IsFailure)
            return renamed;

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
            return DepartmentErrors.NameTaken;
        }

        return Result.Success();
    }
}

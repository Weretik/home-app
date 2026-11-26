using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Departments;
using Shopping.Domain.Items;

namespace Shopping.Application.Features.Departments.Commands.ReorderDepartments;

/// <summary>Numbers the departments 10, 20, 30… in the order sent. Each row's version guards a concurrent reorder.</summary>
public sealed class ReorderDepartmentsHandler(IShoppingDbContext db) : ICommandHandler<ReorderDepartmentsCommand, Result>
{
    public async ValueTask<Result> Handle(ReorderDepartmentsCommand c, CancellationToken cancellationToken)
    {
        var departments = await db.Departments.ToDictionaryAsync(d => d.Id, cancellationToken);
        if (departments.Count != c.DepartmentIds.Count || !c.DepartmentIds.All(departments.ContainsKey))
            return DepartmentErrors.OrderIncomplete;

        for (var index = 0; index < c.DepartmentIds.Count; index++)
            departments[c.DepartmentIds[index]].MoveTo((index + 1) * Department.SortStep);

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

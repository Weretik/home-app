using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Todos.Domain.Categories;
using Todos.Domain.Tasks;

namespace Todos.Application.Features.Categories.Commands.UpdateTodoCategory;

public sealed class UpdateTodoCategoryHandler(ITodosDbContext db) : ICommandHandler<UpdateTodoCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateTodoCategoryCommand c, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == new TodoCategoryId(c.Id), cancellationToken);
        if (category is null)
            return CategoryErrors.NotFound(new TodoCategoryId(c.Id));
        if (category.Version != c.Version)
            return TodoErrors.ChangedMeanwhile;

        var name = c.Name.Trim().ToLowerInvariant();
        if (await db.Categories.AnyAsync(x => x.Id != new TodoCategoryId(c.Id) && x.Name.ToLower() == name, cancellationToken))
            return CategoryErrors.NameTaken;

        var renamed = category.Rename(c.Name, c.Emoji);
        if (renamed.IsFailure)
            return renamed;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return TodoErrors.ChangedMeanwhile;
        }
        catch (UniqueConstraintViolationException)
        {
            return CategoryErrors.NameTaken;
        }

        return Result.Success();
    }
}

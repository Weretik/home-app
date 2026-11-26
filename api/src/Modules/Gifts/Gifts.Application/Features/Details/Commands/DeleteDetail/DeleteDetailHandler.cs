using BuildingBlocks.Domain.Results;
using Gifts.Domain.People;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Details.Commands.DeleteDetail;

public sealed class DeleteDetailHandler(IGiftsDbContext db) : ICommandHandler<DeleteDetailCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteDetailCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.Details.Where(x => x.Id == c.Id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? DetailErrors.NotFound(c.Id) : Result.Success();
    }
}

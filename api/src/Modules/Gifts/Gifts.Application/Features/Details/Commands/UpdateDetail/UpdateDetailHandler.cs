using BuildingBlocks.Domain.Results;
using Gifts.Domain.People;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Details.Commands.UpdateDetail;

public sealed class UpdateDetailHandler(IGiftsDbContext db) : ICommandHandler<UpdateDetailCommand, Result>
{
    public async ValueTask<Result> Handle(UpdateDetailCommand c, CancellationToken cancellationToken)
    {
        var detail = await db.Details.FirstOrDefaultAsync(x => x.Id == new DetailId(c.Id), cancellationToken);
        if (detail is null)
            return DetailErrors.NotFound(new DetailId(c.Id));
        if (detail.Version != c.Version)
            return DetailErrors.ChangedMeanwhile;

        var updated = detail.Update(c.Recipient, c.Label, c.Value);
        if (updated.IsFailure)
            return updated;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return DetailErrors.ChangedMeanwhile;
        }

        return Result.Success();
    }
}

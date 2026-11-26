using BuildingBlocks.Domain.Results;
using Gifts.Domain.Wishlist;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Gifts.Application.Features.Items.Commands.DeleteGift;

/// <summary>One <c>DELETE</c>; the database removes the variants with it (<c>ON DELETE CASCADE</c>).</summary>
public sealed class DeleteGiftHandler(IGiftsDbContext db) : ICommandHandler<DeleteGiftCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteGiftCommand c, CancellationToken cancellationToken)
    {
        var deleted = await db.Gifts.Where(x => x.Id == c.Id).ExecuteDeleteAsync(cancellationToken);
        return deleted == 0 ? GiftErrors.NotFound(c.Id) : Result.Success();
    }
}

using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Shopping.Domain.Settings;

namespace Shopping.Application.Features.Settings.Queries.GetSettings;

public sealed class GetSettingsHandler(IShoppingDbContext db) : IQueryHandler<GetSettingsQuery, Result<ShoppingSettingsView>>
{
    public async ValueTask<Result<ShoppingSettingsView>> Handle(GetSettingsQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.Settings.AsNoTracking()
            .Select(s => new ShoppingSettingsView(s.CartKeepHours, s.FrequentDays, s.Version))
            .FirstOrDefaultAsync(cancellationToken);
        return settings is null ? ShoppingSettingsErrors.Missing : settings;
    }
}

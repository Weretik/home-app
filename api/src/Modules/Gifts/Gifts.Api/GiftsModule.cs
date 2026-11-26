using Gifts.Api.Endpoints.Categories;
using Gifts.Api.Endpoints.Details;
using Gifts.Api.Endpoints.Items;
using Gifts.Api.Endpoints.Status;
using Gifts.Application;
using Gifts.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gifts.Api;

/// <summary>
/// The Gifts module: the gift wishlist of the household (the old «🎁 Список подарков» sheet), its archive, categories
/// and the personal details. The host calls <see cref="AddGiftsModule"/> and <see cref="MapGiftsModule"/>; nothing
/// else of the module is visible to it.
/// </summary>
public static class GiftsModule
{
    public static IServiceCollection AddGiftsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddGiftsApplication();
        services.AddGiftsInfrastructure(configuration);
        return services;
    }

    public static IEndpointRouteBuilder MapGiftsModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api/gifts")
            .WithTags("Gifts")
            .MapListGifts()
            .MapCreateGift()
            .MapUpdateGift()
            .MapDeleteGift()
            .MapChangeGiftStatus()
            .MapGetGiftsSummary()
            .MapListGiftCategories()
            .MapCreateGiftCategory()
            .MapRenameGiftCategory()
            .MapDeleteGiftCategory()
            .MapListGiftDetails()
            .MapCreateGiftDetail()
            .MapUpdateGiftDetail()
            .MapDeleteGiftDetail();
        return endpoints;
    }
}

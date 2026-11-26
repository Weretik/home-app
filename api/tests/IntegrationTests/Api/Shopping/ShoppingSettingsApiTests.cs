using System.Net;
using IntegrationTests.TestHost;
using static IntegrationTests.Api.Shopping.ShoppingRequests;

namespace IntegrationTests.Api.Shopping;

/// <summary>The module settings end to end; the test puts the defaults back.</summary>
[Collection(ApiCollection.Name)]
public sealed class ShoppingSettingsApiTests(ApiFactory api)
{
    [SkippableFact]
    public async Task Settings_AreChecked_AndGuardedByTheirVersion()
    {
        Skip.If(api.Unavailable is not null, api.Unavailable);
        using var client = api.CreateHttpsClient();
        var settings = await GetAsync(client, "/settings");
        Assert.Equal(3, settings.GetProperty("cartKeepHours").GetInt32());
        Assert.Equal(183, settings.GetProperty("frequentDays").GetInt32());

        var tooMany = await PutAsync(client, "/settings", new { cartKeepHours = 73, frequentDays = 183 }, settings.Version());
        var noVersion = await PutAsync(client, "/settings", new { cartKeepHours = 5, frequentDays = 90 });
        var changed = await PutAsync(client, "/settings", new { cartKeepHours = 5, frequentDays = 90 }, settings.Version());
        var stale = await PutAsync(client, "/settings", new { cartKeepHours = 6, frequentDays = 90 }, settings.Version());
        var after = await GetAsync(client, "/settings");
        var restored = await PutAsync(client, "/settings", new { cartKeepHours = 3, frequentDays = 183 }, after.Version());

        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noVersion.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(5, after.GetProperty("cartKeepHours").GetInt32());
        Assert.Equal(90, after.GetProperty("frequentDays").GetInt32());
        Assert.Equal(HttpStatusCode.NoContent, restored.StatusCode);
    }
}

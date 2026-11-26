using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace Host.Api.Composition;

/// <summary>
/// Production runs behind a reverse proxy or load balancer that ends TLS. With <c>ForwardedHeaders:Enabled</c> the
/// API takes the client address and scheme from <c>X-Forwarded-For</c> and <c>X-Forwarded-Proto</c>, but only
/// from the proxies in <c>ForwardedHeaders:KnownProxies</c> and <c>:KnownNetworks</c> (loopback when both are empty).
/// The client address feeds the rate limits; the scheme keeps secure cookies and generated links on HTTPS.
/// </summary>
internal static class ReverseProxySetup
{
    public const string SectionName = "ForwardedHeaders";

    public static IServiceCollection AddReverseProxySupport(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var proxy in section.GetSection("KnownProxies").Get<string[]>() ?? [])
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            foreach (var network in section.GetSection("KnownNetworks").Get<string[]>() ?? [])
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        });
        return services;
    }

    /// <summary>Reads the forwarded headers first, then sends HSTS outside Development.</summary>
    public static WebApplication UseReverseProxySupport(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>($"{SectionName}:Enabled"))
            app.UseForwardedHeaders();
        if (!app.Environment.IsDevelopment())
            app.UseHsts();
        return app;
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using BuildingBlocks.Api.Scheduling;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Api.Authentication;

/// <summary>
/// The <see cref="IdentitySchemes.SchedulerKey"/> scheme: a <c>GET</c> or <c>HEAD</c> with the scheduler key is the
/// read-only service <c>scheduler</c>, so the post-deploy checks can read every section (R-010). Any other method
/// gets no identity from the key; without a configured key the header is ignored.
/// </summary>
internal sealed class SchedulerKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<OutboxTickOptions> tick)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string ServiceName = "scheduler";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(SchedulerKey.HeaderName)
            || !(HttpMethods.IsGet(Request.Method) || HttpMethods.IsHead(Request.Method))
            || string.IsNullOrEmpty(tick.Value.Key))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (SchedulerKey.Refusal(Context, tick.Value.Key) is not null)
            return Task.FromResult(AuthenticateResult.Fail("Wrong scheduler key."));

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, ServiceName)], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}

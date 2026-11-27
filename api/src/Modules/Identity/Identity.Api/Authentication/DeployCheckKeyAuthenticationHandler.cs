using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Api.Authentication;

/// <summary>
/// The <see cref="IdentitySchemes.DeployCheckKey"/> scheme: a <c>GET</c> or <c>HEAD</c> with the deploy check key is
/// the read-only service <c>deploy-check</c>, so the post-deploy checks can read every section (R-010, ADR 0045). The
/// key is its own: the scheduler key runs the ticks and reads nothing. Any other method gets no identity from the key;
/// without a configured key the header is ignored.
/// </summary>
internal sealed class DeployCheckKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<DeployCheckOptions> deployCheck)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string ServiceName = "deploy-check";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var expected = deployCheck.Value.Key;
        if (!Request.Headers.TryGetValue(DeployCheckOptions.HeaderName, out var sent)
            || !(HttpMethods.IsGet(Request.Method) || HttpMethods.IsHead(Request.Method))
            || string.IsNullOrEmpty(expected))
            return Task.FromResult(AuthenticateResult.NoResult());

        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(sent.ToString()), Encoding.UTF8.GetBytes(expected)))
            return Task.FromResult(AuthenticateResult.Fail("Wrong deploy check key."));

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, ServiceName)], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}

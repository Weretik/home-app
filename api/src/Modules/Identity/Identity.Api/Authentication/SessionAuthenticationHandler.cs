using System.Text.Encodings.Web;
using Identity.Application.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Identity.Api.Authentication;

/// <summary>
/// The <see cref="IdentitySchemes.Session"/> scheme: a request with a bearer token of an active session of a member
/// on the allowlist is that member (R-006). No header means no result, so another scheme may still answer.
/// </summary>
internal sealed class SessionAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    SessionAuthenticator authenticator)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (MemberClaims.BearerToken(Request) is not { } token)
            return AuthenticateResult.NoResult();

        var member = await authenticator.AuthenticateAsync(token, Context.RequestAborted);
        return member is null
            ? AuthenticateResult.Fail("The session is not active.")
            : AuthenticateResult.Success(new AuthenticationTicket(MemberClaims.Principal(member, Scheme.Name), Scheme.Name));
    }
}

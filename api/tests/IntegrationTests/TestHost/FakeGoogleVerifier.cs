using Identity.Application.Google;

namespace IntegrationTests.TestHost;

/// <summary>
/// Stands in for Google in the tests: the credential <c>google:&lt;email&gt;</c> is a verified token of that e-mail,
/// <c>google-unverified:&lt;email&gt;</c> one whose e-mail Google has not verified, and anything else is invalid.
/// </summary>
internal sealed class FakeGoogleVerifier : IGoogleTokenVerifier
{
    public static string Credential(string email) => $"google:{email}";

    public static string UnverifiedCredential(string email) => $"google-unverified:{email}";

    public Task<GoogleIdentity?> VerifyAsync(string credential, CancellationToken cancellationToken)
    {
        var parts = credential.Split(':', 2);
        GoogleIdentity? identity = parts switch
        {
            ["google", var email] => new GoogleIdentity(email, true, $"Тест {email.Split('@')[0]}", "https://lh3.example/photo.png"),
            ["google-unverified", var email] => new GoogleIdentity(email, false, null, null),
            _ => null,
        };
        return Task.FromResult(identity);
    }
}

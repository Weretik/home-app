namespace Identity.Api.Authentication;

/// <summary>
/// Settings of the deploy check key (section <c>DeployCheck</c>, ADR 0045): the post-deploy smoke test sends it in
/// <see cref="HeaderName"/> to read every section. Empty turns the key off. Keep it in the App Service settings and the
/// GitHub secret <c>DEPLOY_CHECK_KEY</c>, never in appsettings.
/// </summary>
public sealed class DeployCheckOptions
{
    public const string SectionName = "DeployCheck";

    public const string HeaderName = "X-Deploy-Check-Key";

    public string Key { get; set; } = string.Empty;
}

namespace Identity.Api.Authentication;

/// <summary>The authentication schemes of the API (ADR 0037).</summary>
public static class IdentitySchemes
{
    /// <summary>A family member's session: <c>Authorization: Bearer &lt;token&gt;</c>.</summary>
    public const string Session = "Session";

    /// <summary>The deploy checks: a <c>GET</c> with <c>X-Scheduler-Key</c>, read only (R-010).</summary>
    public const string SchedulerKey = "SchedulerKey";
}

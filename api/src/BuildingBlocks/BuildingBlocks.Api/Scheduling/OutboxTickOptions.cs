namespace BuildingBlocks.Api.Scheduling;

/// <summary>Settings of the outbox tick endpoint (section <c>OutboxTick</c>).</summary>
public sealed class OutboxTickOptions
{
    public const string SectionName = "OutboxTick";

    /// <summary>
    /// The shared key Cloud Scheduler sends in the <c>X-Scheduler-Key</c> header. Empty turns the endpoint off.
    /// Keep it in a secret store, never in appsettings.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}

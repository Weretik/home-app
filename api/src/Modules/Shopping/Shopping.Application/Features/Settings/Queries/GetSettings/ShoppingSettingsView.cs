namespace Shopping.Application.Features.Settings.Queries.GetSettings;

/// <summary>How long bought items stay in the cart, how far back frequent items count, and the version to edit with.</summary>
public sealed record ShoppingSettingsView(int CartKeepHours, int FrequentDays, uint Version);

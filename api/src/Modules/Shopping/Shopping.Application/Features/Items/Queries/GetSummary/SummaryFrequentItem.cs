namespace Shopping.Application.Features.Items.Queries.GetSummary;

/// <summary>A frequent name and how many times it was bought in the window.</summary>
public sealed record SummaryFrequentItem(string Name, int TimesBought);

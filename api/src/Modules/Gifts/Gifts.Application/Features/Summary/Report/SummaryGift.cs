namespace Gifts.Application.Features.Summary.Report;

/// <summary>A wanted «🔥» gift in a recipient's top list.</summary>
public sealed record SummaryGift(Guid Id, string Name, decimal? Price, string? Link);

using Gifts.Domain.People;

namespace Gifts.Application.Features.Summary.Report;

/// <summary>A wanted gift whose occasion is today or later.</summary>
public sealed record UpcomingOccasion(Guid Id, Recipient Recipient, string Name, string? Occasion, DateOnly OccasionDate);

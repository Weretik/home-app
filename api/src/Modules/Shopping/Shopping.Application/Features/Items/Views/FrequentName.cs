namespace Shopping.Application.Features.Items.Views;

/// <summary>A name bought often: the latest spelling and department, how many times, and when last.</summary>
public sealed record FrequentName(string Name, Guid DepartmentId, int TimesBought, DateTimeOffset LastBoughtAtUtc);

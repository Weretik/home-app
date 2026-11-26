using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Settings.Queries.GetSettings;

/// <summary>The module settings with their version.</summary>
public sealed record GetSettingsQuery : IQuery<Result<ShoppingSettingsView>>;

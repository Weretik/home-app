using BuildingBlocks.Domain.Results;
using Mediator;

namespace Shopping.Application.Features.Settings.Commands.UpdateSettings;

/// <summary>Sets the cart hours and the frequent window, if nobody changed them since <see cref="Version"/>.</summary>
public sealed record UpdateSettingsCommand(uint Version, int CartKeepHours, int FrequentDays) : ICommand<Result>;

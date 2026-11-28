using BuildingBlocks.Domain.Results;
using Mediator;

namespace Identity.Application.Features.Sessions.Commands.SignOutEverywhere;

/// <summary>Ends every session of the member whose session <see cref="Token"/> is, this one included (ADR 0046).</summary>
public sealed record SignOutEverywhereCommand(string Token) : ICommand<Result>;

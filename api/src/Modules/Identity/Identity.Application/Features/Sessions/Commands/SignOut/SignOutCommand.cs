using BuildingBlocks.Domain.Results;
using Mediator;

namespace Identity.Application.Features.Sessions.Commands.SignOut;

/// <summary>Ends the session of <see cref="Token"/> (R-007).</summary>
public sealed record SignOutCommand(string Token) : ICommand<Result>;

using BuildingBlocks.Domain.Results;
using Mediator;

namespace Identity.Application.Features.Sessions.Commands.SignIn;

/// <summary>Signs a family member in with a Google ID token and starts a session (spec identity/001, R-001…R-004).</summary>
public sealed record SignInCommand(string Credential) : ICommand<Result<IdentitySession>>;

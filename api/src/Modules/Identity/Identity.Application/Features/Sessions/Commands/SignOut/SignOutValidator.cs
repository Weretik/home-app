using FluentValidation;
using Identity.Application.Authentication;

namespace Identity.Application.Features.Sessions.Commands.SignOut;

internal sealed class SignOutValidator : AbstractValidator<SignOutCommand>
{
    public SignOutValidator() => RuleFor(c => c.Token).NotEmpty().MaximumLength(SessionAuthenticator.MaxTokenLength);
}

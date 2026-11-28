using FluentValidation;
using Identity.Application.Authentication;

namespace Identity.Application.Features.Sessions.Commands.SignOutEverywhere;

internal sealed class SignOutEverywhereValidator : AbstractValidator<SignOutEverywhereCommand>
{
    public SignOutEverywhereValidator() =>
        RuleFor(c => c.Token).NotEmpty().MaximumLength(SessionAuthenticator.MaxTokenLength);
}

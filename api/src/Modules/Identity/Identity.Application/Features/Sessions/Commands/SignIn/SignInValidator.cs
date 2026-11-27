using FluentValidation;

namespace Identity.Application.Features.Sessions.Commands.SignIn;

internal sealed class SignInValidator : AbstractValidator<SignInCommand>
{
    /// <summary>Google ID tokens are about 1–2 KB.</summary>
    public const int MaxCredentialLength = 8192;

    public SignInValidator() => RuleFor(c => c.Credential).NotEmpty().MaximumLength(MaxCredentialLength);
}

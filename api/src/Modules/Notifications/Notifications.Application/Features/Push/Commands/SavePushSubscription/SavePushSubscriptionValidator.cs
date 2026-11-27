using FluentValidation;
using Notifications.Domain.Push;

namespace Notifications.Application.Features.Push.Commands.SavePushSubscription;

internal sealed class SavePushSubscriptionValidator : AbstractValidator<SavePushSubscriptionCommand>
{
    public SavePushSubscriptionValidator()
    {
        RuleFor(c => c.Endpoint)
            .NotEmpty().WithMessage("Вкажіть адресу підписки.")
            .MaximumLength(PushSubscription.MaxEndpointLength).WithMessage("Адреса підписки задовга.")
            .Must(BeHttps).WithMessage("Адреса підписки має бути https-адресою.");
        RuleFor(c => c.P256dh)
            .Must(k => DecodedLength(k) == 65).WithMessage("Ключ підписки (p256dh) має бути ключем P-256 у base64url.");
        RuleFor(c => c.Auth)
            .Must(k => DecodedLength(k) == 16).WithMessage("Секрет підписки (auth) має бути 16 байтами у base64url.");
    }

    private static bool BeHttps(string endpoint) =>
        Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

    /// <summary>The byte length of a base64url (or base64) text, or -1 when it is not one.</summary>
    internal static int DecodedLength(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > PushSubscription.MaxP256dhLength)
            return -1;
        var base64 = text.Trim().TrimEnd('=').Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');
        var buffer = new byte[base64.Length];
        return Convert.TryFromBase64String(base64, buffer, out var written) ? written : -1;
    }
}

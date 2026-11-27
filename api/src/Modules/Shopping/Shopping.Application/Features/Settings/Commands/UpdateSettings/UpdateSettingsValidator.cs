using FluentValidation;
using Shopping.Domain.Settings;

namespace Shopping.Application.Features.Settings.Commands.UpdateSettings;

internal sealed class UpdateSettingsValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsValidator()
    {
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Надішліть версію налаштувань у заголовку If-Match.");
        RuleFor(c => c.CartKeepHours).InclusiveBetween(ShoppingSettings.MinCartKeepHours, ShoppingSettings.MaxCartKeepHours);
        RuleFor(c => c.FrequentDays).InclusiveBetween(ShoppingSettings.MinFrequentDays, ShoppingSettings.MaxFrequentDays);
    }
}

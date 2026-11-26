using FluentValidation;
using Shopping.Domain.Settings;

namespace Shopping.Application.Features.Settings.Commands.UpdateSettings;

internal sealed class UpdateSettingsValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsValidator()
    {
        RuleFor(c => c.Version).NotEqual(0u).WithMessage("Send the version of the settings in the If-Match header.");
        RuleFor(c => c.CartKeepHours).InclusiveBetween(ShoppingSettings.MinCartKeepHours, ShoppingSettings.MaxCartKeepHours);
        RuleFor(c => c.FrequentDays).InclusiveBetween(ShoppingSettings.MinFrequentDays, ShoppingSettings.MaxFrequentDays);
    }
}

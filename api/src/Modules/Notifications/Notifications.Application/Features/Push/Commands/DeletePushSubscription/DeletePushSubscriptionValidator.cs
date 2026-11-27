using FluentValidation;

namespace Notifications.Application.Features.Push.Commands.DeletePushSubscription;

internal sealed class DeletePushSubscriptionValidator : AbstractValidator<DeletePushSubscriptionCommand>
{
    public DeletePushSubscriptionValidator() => RuleFor(c => c.Id).NotEmpty();
}

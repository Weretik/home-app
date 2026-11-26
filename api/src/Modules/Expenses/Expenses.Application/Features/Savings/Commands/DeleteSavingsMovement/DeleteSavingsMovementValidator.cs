using FluentValidation;

namespace Expenses.Application.Features.Savings.Commands.DeleteSavingsMovement;

internal sealed class DeleteSavingsMovementValidator : AbstractValidator<DeleteSavingsMovementCommand>
{
    public DeleteSavingsMovementValidator() => RuleFor(c => c.Id).NotEmpty();
}

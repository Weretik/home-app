using Expenses.Application.Idempotency;
using Expenses.Domain.Entries;
using FluentValidation;

namespace Expenses.Application.Features.Savings.Commands.CreateSavingsMovement;

internal sealed class CreateSavingsMovementValidator : AbstractValidator<CreateSavingsMovementCommand>
{
    public CreateSavingsMovementValidator()
    {
        RuleFor(c => c.IdempotencyKey).NotEmpty().MaximumLength(CreateRequest.MaxKeyLength);
        RuleFor(c => c.Person).IsInEnum();
        RuleFor(c => c.Amount).NotEqual(0);
        RuleFor(c => c.Comment).MaximumLength(EntryRules.MaxCommentLength);
    }
}

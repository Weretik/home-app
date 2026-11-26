using BuildingBlocks.Application.Pipeline;
using BuildingBlocks.Domain.Results;
using FluentValidation;
using Mediator;

namespace UnitTests.BuildingBlocks;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Invalid_Input_ReturnsAValidationError_PerField_WithoutCallingTheHandler()
    {
        var behavior = new ValidationBehavior<RenameCommand, Result>([new RenameValidator()]);
        var called = false;

        var result = await behavior.Handle(new RenameCommand(""), (_, _) =>
        {
            called = true;
            return ValueTask.FromResult(Result.Success());
        }, CancellationToken.None);

        Assert.False(called);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Contains("Name", result.Error.Fields!.Keys);
    }

    [Fact]
    public async Task Valid_Input_ReachesTheHandler()
    {
        var behavior = new ValidationBehavior<RenameCommand, Result>([new RenameValidator()]);

        var result = await behavior.Handle(new RenameCommand("Milk"), (_, _) => ValueTask.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    public sealed record RenameCommand(string Name) : ICommand<Result>;

    private sealed class RenameValidator : AbstractValidator<RenameCommand>
    {
        public RenameValidator() => RuleFor(command => command.Name).NotEmpty();
    }
}

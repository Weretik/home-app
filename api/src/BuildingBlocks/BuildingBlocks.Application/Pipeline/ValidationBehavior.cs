using BuildingBlocks.Domain.Results;
using FluentValidation;
using Mediator;

namespace BuildingBlocks.Application.Pipeline;

/// <summary>
/// Runs every FluentValidation validator of a command or query before its handler. Invalid input never reaches
/// the handler: it comes back as a failed <see cref="Result"/> with a message per field.
/// </summary>
public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(message, cancellationToken);

        var context = new ValidationContext<TMessage>(message);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).ToList();
        if (failures.Count == 0)
            return await next(message, cancellationToken);

        var error = new Error(
            "validation",
            failures[0].ErrorMessage,
            ErrorType.Validation,
            failures
                .GroupBy(f => f.PropertyName, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray(), StringComparer.Ordinal));

        return FailedResult<TResponse>.Create is { } create
            ? create(error)
            : throw new ValidationException(failures);
    }
}

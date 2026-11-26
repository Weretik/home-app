using System.Diagnostics;
using BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Pipeline;

/// <summary>Logs how long each command or query took and which business error it returned, if any.</summary>
public sealed partial class LoggingBehavior<TMessage, TResponse>(ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var response = await next(message, cancellationToken);
        var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;

        if (response is Result { Error: { } error })
            LogFailed(logger, typeof(TMessage).Name, error.Code, elapsed);
        else
            LogHandled(logger, typeof(TMessage).Name, elapsed);

        return response;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "{Message} handled in {ElapsedMs:0} ms")]
    private static partial void LogHandled(ILogger logger, string message, double elapsedMs);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Message} returned {ErrorCode} in {ElapsedMs:0} ms")]
    private static partial void LogFailed(ILogger logger, string message, string errorCode, double elapsedMs);
}

using BuildingBlocks.Domain.Results;
using Mediator;
using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Chats.Commands.SendTestMessage;

/// <summary>Sends «✅ Бот на связи» to the chat of <see cref="Audience"/> at once (spec notifications/001, R-018).</summary>
public sealed record SendTestMessageCommand(Audience Audience) : ICommand<Result>;

using BuildingBlocks.Domain.Results;
using Mediator;
using Notifications.Domain.Chats;

namespace Notifications.Application.Features.Chats.Commands.UnlinkChat;

/// <summary>Frees the slot of <see cref="Audience"/>, so another chat can be linked (spec notifications/001, R-005).</summary>
public sealed record UnlinkChatCommand(Audience Audience) : ICommand<Result>;

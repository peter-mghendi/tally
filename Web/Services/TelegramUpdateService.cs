using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Web.Channels;
using Web.Data;
using Web.Hubs;
using Web.Models;
using Poll = Telegram.Bot.Types.Poll;

namespace Web.Services;

public sealed class TelegramUpdateService
{
    private readonly IHubContext<TallyHub, TallyHub.ITallyHubClient> _hubContext;
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<TelegramUpdateService> _logger;
    private readonly TallyContext _context;
    private readonly TelegramChannel _channel;

    public TelegramUpdateService(
        IHubContext<TallyHub, TallyHub.ITallyHubClient> hubContext,
        ITelegramBotClient botClient,
        ILogger<TelegramUpdateService> logger,
        TallyContext context,
        TelegramChannel channel
    )
    {
        _hubContext = hubContext;
        _botClient = botClient;
        _logger = logger;
        _context = context;
        _channel = channel;
    }

    public async Task HandleAsync(Update update, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Update {UpdateId} received from Telegram", update.Id);
        var handler = update.Type switch
        {
            UpdateType.Poll => BotOnPollRequested(update.Poll!),
            UpdateType.PollAnswer => BotOnPollAnswered(update.PollAnswer!, cancellationToken),
            UpdateType.Message => BotOnMessageReceived(update.Message!, cancellationToken),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!, cancellationToken),
            _ => Task.Run(() => _logger.LogInformation($"Unsupported update type received: {update.Type.ToString()}."), cancellationToken)
        };
        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            HandleError(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received message type: {messageType}", message.Type);
        if (message.Type != MessageType.Text)
            return;

        var chat = message.Chat;
        var reply =
            $"Hi, {chat.Username} you cannot interact with this bot directly. Visit the repo on GitHub for more info,";
        _ = await _botClient.SendTextMessageAsync(chat.Id, reply, cancellationToken: cancellationToken);
    }

    private Task BotOnPollRequested(Poll poll)
    {
        _logger.LogInformation("Poll {Question} was sent with id: {SentMessageId}", poll.Question,  poll.Id);
        return Task.CompletedTask;
    }

    private async Task BotOnPollAnswered(PollAnswer pollAnswer, CancellationToken cancellationToken = default)
    {
        var channelPoll = await _context.ChannelPolls
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Options)
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.LiveVotes)
            .SingleAsync(cp => cp.AuxiliaryIdentifier == pollAnswer.PollId && cp.Channel == PollChannel.Telegram,
                cancellationToken);
        var poll = channelPoll.Poll;

        if (poll.EndedAt is not null)
        {
            _logger.LogInformation("Discarding Telegram vote for poll {Poll}: Poll has concluded.", poll.Id);
            return;
        }
        
        var userIdentifier = pollAnswer.User.Id.ToString();

        _logger.LogInformation("Received Telegram vote for poll: {Poll}", poll.Id);

        if (pollAnswer.OptionIds.Length <= 0)
        {
            poll.LiveVotes.Remove(poll.LiveVotes
                .Single(v => v.Channel == PollChannel.Telegram && v.UserIdentifier == userIdentifier));
        }
        else
        {
            poll.LiveVotes.Add(new LiveVote
            {
                Channel = PollChannel.Telegram,
                Option = poll.Options[pollAnswer.OptionIds[0]],
                UserIdentifier = userIdentifier
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _hubContext.Clients.Group(poll.Id.ToString())
            .UpdateResult(nameof(PollChannel.Telegram), await _channel.CountVotesAsync(channelPoll, cancellationToken));
    }

    private void HandleError(Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error: [{apiRequestException.ErrorCode}]: {apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
    }
}
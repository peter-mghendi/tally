using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Web.Data;
using Web.Models;
using Poll = Telegram.Bot.Types.Poll;

namespace Web.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly TallyContext _context;

    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger, TallyContext context)
    {
        _botClient = botClient;
        _logger = logger;
        _context = context;
    }

    public async Task HandleAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Poll => BotOnPollRequested(update.Poll!),
            UpdateType.PollAnswer => BotOnPollAnswered(update.PollAnswer!),
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
            _ => Task.Run(() => _logger.LogInformation("Unsupported update type received."))
        };
        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {messageType}", message.Type);
        if (message.Type != MessageType.Text)
            return;

        var chat = message.Chat;
        var reply = await _botClient.SendTextMessageAsync(chat.Id, $"Hi, {chat.Username} you cannot interact with this bot directly.");
        _logger.LogInformation("The message was sent with id: {sentMessageId}", reply.MessageId);
    }
    
    private Task BotOnPollRequested(Poll poll)
    {
        _logger.LogInformation("Poll question: {messageType}", poll.Question);
        _logger.LogInformation("The poll was sent with id: {sentMessageId}", poll.Id);
        return Task.CompletedTask;
    }
    
    private async Task BotOnPollAnswered(PollAnswer pollAnswer)
    {
        var channelPoll = await _context.ChannelPolls
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Options)
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Votes)
            .SingleAsync(cp => cp.Identifier == pollAnswer.PollId && cp.Channel == PollChannel.Telegram);
        var poll = channelPoll.Poll;
        var userIdentifier = pollAnswer.User.Id.ToString();

        if (pollAnswer.OptionIds.Length <= 0)
        {
            var vote = poll.Votes.Single(v => v.Channel == PollChannel.Telegram && v.UserIdentifier == userIdentifier);
            poll.Votes.Remove(vote);
        }
        else
        {
            poll.Votes.Add(new Vote
            {
                Channel = PollChannel.Telegram,
                Option = poll.Options[pollAnswer.OptionIds[0]],
                UserIdentifier = userIdentifier
            });
        }

        await _context.SaveChangesAsync();
    }

    private Task HandleErrorAsync(Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }
}
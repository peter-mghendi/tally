using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Web.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public HandleUpdateService(ITelegramBotClient botClient, ILogger<HandleUpdateService> logger)
    {
        _botClient = botClient;
        _logger = logger;
    }

    public async Task EchoAsync(Update update)
    {
        var handler = update.Type switch
        {
            UpdateType.Poll => BotOnPollRequested(update.Poll!),
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
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
        _logger.LogInformation("Chat ID: {messageType}", message.Chat.Id);
        if (message.Type != MessageType.Text)
            return;
        
        Message pollMessage = await _botClient.SendPollAsync(
            chatId: message.Chat.Id,
            question: "Do you know... The Muffin Man?",
            options: new []
            {
                "The Muffin Man?",
                "Who lives on Drury Lane?"
            });
        _logger.LogInformation("The message was sent with id: {sentMessageId}", pollMessage.MessageId);
    }
    
    private async Task BotOnPollRequested(Poll poll)
    {
        _logger.LogInformation("Poll question: {messageType}", poll.Question);
        // if (message.Type != MessageType.Text)
        //     return;
        //
        // Message pollMessage = await _botClient.SendPollAsync(
        //     chatId: message.Chat.Id,
        //     question: "Do you know... The Muffin Man?",
        //     options: new []
        //     {
        //         "The Muffin Man?",
        //         "Who lives on Drury Lane?"
        //     });
        _logger.LogInformation("The message was sent with id: {sentMessageId}", poll.Id);
    }


    public Task HandleErrorAsync(Exception exception)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }
}
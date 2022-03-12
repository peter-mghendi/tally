using Telegram.Bot;
using Web.Models;
using Web.Models.Configuration;

namespace Web.Channels;

public class TelegramChannel : Channel
{
    private readonly ITelegramBotClient _botClient;
    private readonly long _chatId;

    public TelegramChannel(ITelegramBotClient botClient, IConfiguration configuration)
    {
        _botClient = botClient;
        _chatId = configuration.GetSection("TelegramBotConfiguration").Get<TelegramBotConfiguration>().ChatId;
    }

    public override PollChannel PollChannel => PollChannel.Telegram;

    public override async Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default)
    {
        var pollMessage = await _botClient.SendPollAsync(
            chatId: _chatId,
            question: question,
            options: options,
            isAnonymous: false,
            cancellationToken: cancellationToken
        );

        return BuildPoll(pollMessage.Poll!.Id);
    }
}
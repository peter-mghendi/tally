using Telegram.Bot;
using Web.Models;

namespace Web.Channels;

public class TelegramChannel
{
    private readonly ITelegramBotClient _botClient;

    public TelegramChannel(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default)
    {
        var pollMessage = await _botClient.SendPollAsync(
            332474019,
            question: question,
            options: options,
            isAnonymous: false,
            cancellationToken: cancellationToken
        );

        var telegramPoll = new ChannelPoll
        {
            Channel = PollChannel.Telegram,
            Identifier = pollMessage.Poll!.Id
        };

        return telegramPoll;
    }
}
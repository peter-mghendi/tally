using Telegram.Bot;
using Web.Data;
using Web.Models;

namespace Web.Channels;

public class TelegramChannel
{
    private readonly TallyContext _context;
    private readonly ITelegramBotClient _botClient;

    public TelegramChannel(ITelegramBotClient botClient, TallyContext context)
    {
        _botClient = botClient;
        _context = context;
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
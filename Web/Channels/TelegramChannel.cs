using Telegram.Bot;
using Telegram.Bot.Types;
using Web.Data;
using Web.Models;
using Poll = Web.Models.Poll;

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
        Message pollMessage = await _botClient.SendPollAsync(332474019, question, options, cancellationToken: cancellationToken);

        var telegramPoll = new ChannelPoll
        {
            Channel = ChannelPoll.PollChannel.Telegram,
            Identifier = pollMessage.MessageId.ToString()
        };

        return telegramPoll;
    }
}
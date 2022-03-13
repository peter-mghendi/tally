using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Telegram.Bot;
using Web.Data;
using Web.Models;
using Web.Models.Configuration;

namespace Web.Channels;

public class TelegramChannel : Channel
{
    private readonly ITelegramBotClient _botClient;
    private readonly long _chatId;
    private readonly TallyContext _tallyContext;

    public TelegramChannel(ITelegramBotClient botClient, IConfiguration configuration, TallyContext tallyContext)
    {
        _botClient = botClient;
        _chatId = configuration.GetSection("TelegramBotConfiguration").Get<TelegramBotConfiguration>().ChatId;
        _tallyContext = tallyContext;
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

    public override async Task<List<PollResult>> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        return await _tallyContext.Options.Where(o => o.Poll.Id == channelPoll.Poll.Id)
            .Select(o => new PollResult(o.Id, o.Votes.Count)).ToListAsync(cancellationToken);
    }
}
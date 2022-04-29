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
        _chatId = configuration.GetRequiredSection(nameof(TelegramBotConfiguration)).Get<TelegramBotConfiguration>().ChatId;
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

    public override async Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        var query = from option in _tallyContext.Options where option.Poll.Id == channelPoll.Poll.Id
            select new PollResult(option.Id, option.LiveVotes.Count(lv => lv.Channel == PollChannel.Telegram));
        return LiveResult(await query.ToListAsync(cancellationToken));
    }
}
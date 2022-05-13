using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Web.Data;
using Web.Models;
using Web.Models.Configuration;

namespace Web.Channels;

public class TelegramChannel : Channel
{
    private readonly ILogger<TelegramChannel> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly long _chatId;
    private readonly TallyContext _tallyContext;

    public TelegramChannel(
        ILogger<TelegramChannel> logger,
        ITelegramBotClient botClient,
        IConfiguration configuration,
        TallyContext tallyContext
    )
    {
        _logger = logger;
        _botClient = botClient;
        _chatId = configuration.GetRequiredSection(nameof(TelegramBotConfiguration)).Get<TelegramBotConfiguration>()
            .ChatId;
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

        return BuildPoll(pollMessage.MessageId.ToString(), pollMessage.Poll!.Id);
    }

    public override async Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll,
        CancellationToken cancellationToken = default)
    {
        var query = from option in _tallyContext.Options
            where option.Poll.Id == channelPoll.Poll.Id
            select new PollResult(option.Id, option.LiveVotes.Count(lv => lv.Channel == PollChannel.Telegram));
        return LiveResult(await query.ToListAsync(cancellationToken));
    }

    public override async Task ConcludePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        var poll = await _botClient.StopPollAsync(_chatId, int.Parse(channelPoll.PrimaryIdentifier),
            cancellationToken: cancellationToken);
        _logger.LogInformation("Closing Telegram channel poll for poll {Poll}. {Votes} votes were received",
            channelPoll.Poll.Id, poll.TotalVoterCount);

        // TODO: Maybe use this result to verify vote totals?
    }

    public override Task DeletePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
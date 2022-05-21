using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Channels;

public class WebChannel : Channel
{
    private readonly ILogger<WebChannel> _logger;
    private readonly TallyContext _tallyContext;

    public WebChannel(ILogger<WebChannel> logger, TallyContext tallyContext)
    {
        _logger = logger;
        _tallyContext = tallyContext;
    }

    public override PollChannel PollChannel => PollChannel.Web;
    
    public override Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to create Web poll which will be created automatically.");
        return Task.FromResult(BuildPoll("", ""));
    }

    public override async Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        var query = from option in _tallyContext.Options
            where option.Poll.Id == channelPoll.Poll.Id
            select new PollResult(option.Id, option.LiveVotes.Count(lv => lv.Channel == PollChannel));
        return LiveResult(await query.ToListAsync(cancellationToken));
    }

    public override Task ConcludePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to conclude Web poll (ID: {ID}), which will be concluded automatically.", channelPoll.PrimaryIdentifier);
        return Task.CompletedTask;
    }

    public override Task DeletePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting to delete Web poll (ID: {ID}), which will be deleted automatically..", channelPoll.PrimaryIdentifier);
        return Task.CompletedTask;
    }
}
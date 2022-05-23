using System.Text;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Data;
using Tally.Web.Models;
using Tally.Web.Services;

namespace Tally.Web.Channels;

public class DiscordChannel : Channel
{
    private readonly DiscordAdapter _adapter;
    private readonly ILogger<DiscordChannel> _logger;
    private readonly TallyContext _tallyContext;
    
    private readonly string[] _reactions = {":one:", ":two:", ":three:", ":four:"};
    
    public DiscordChannel(DiscordAdapter adapter, ILogger<DiscordChannel> logger, TallyContext tallyContext)
    {
        _adapter = adapter;
        _logger = logger;
        _tallyContext = tallyContext;
    }
    
    public override PollChannel PollChannel => PollChannel.Discord;
    public override async Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options, CancellationToken cancellationToken = default)
    {
        var textBuilder = new StringBuilder($"{question}\n\nReact with one of the following to vote:\n");
        var enumerable = options as string[] ?? options.ToArray();
        for (var i = 0; i < enumerable.Length; i++)
        {
            textBuilder.Append($"{_reactions[i]} {enumerable.ElementAt(i)}\n");
        }

        var message = await _adapter.CreatePollAsync(text: textBuilder.ToString());
        var identifier = message.Id.ToString();
        return BuildPoll(identifier, identifier);
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
        _logger.LogInformation("Attempting to conclude Discord poll (Message ID: {Message}), which cannot be manually concluded.", channelPoll.PrimaryIdentifier);
        return Task.CompletedTask;
    }

    public override async Task DeletePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        await _adapter.DeletePollAsync(ulong.Parse(channelPoll.PrimaryIdentifier));
    }
}
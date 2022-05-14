using Web.Models;

namespace Web.Channels;

public class DiscordChannel : Channel
{
    public override PollChannel PollChannel => PollChannel.Discord;
    public override Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task ConcludePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task DeletePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
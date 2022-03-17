using Web.Models;

namespace Web.Channels;

public abstract class Channel : IChannel
{
    public abstract PollChannel PollChannel { get; }
    
    public abstract Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default);

    public abstract Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default);

    protected ChannelPoll BuildPoll(string identifier) => new() {Channel = PollChannel, Identifier = identifier};

    protected static ChannelResult CachedResult(List<PollResult> results, DateTime lastUpdated)
        => new(results, false, lastUpdated);

    protected static ChannelResult LiveResult(List<PollResult> results) => new(results, true);
}
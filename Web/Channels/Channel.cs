using Web.Models;

namespace Web.Channels;

public abstract class Channel : IChannel
{
    public abstract PollChannel PollChannel { get; }
    
    public abstract Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default);

    public abstract Task<List<PollResult>> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default);

    protected ChannelPoll BuildPoll(string identifier) => new() {Channel = PollChannel, Identifier = identifier};
}
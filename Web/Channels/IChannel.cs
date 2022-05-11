using Web.Models;

namespace Web.Channels;

public interface IChannel
{
    public PollChannel PollChannel { get; }

    Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default);

    Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default);

    Task ConcludePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default);

    Task DeletePollAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default);
}
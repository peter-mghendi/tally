using Web.Models;

namespace Web.Channels;

public interface IChannel
{
    public PollChannel PollChannel { get; }

    Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default);

    Task<List<PollResult>> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default);
}
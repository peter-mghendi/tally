using LinqToTwitter;
using LinqToTwitter.Common;
using Web.Models;

namespace Web.Channels;

public class TwitterChannel : Channel
{
    private readonly TwitterContext _context;
    private readonly ILogger<TwitterChannel> _logger;

    public TwitterChannel(TwitterContext context, ILogger<TwitterChannel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override PollChannel PollChannel => PollChannel.Twitter;

    public override async Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default)
    {
        var pollTweet = await _context.TweetPollAsync(
            text: question,
            duration: (int) TimeSpan.FromDays(7).TotalMinutes,
            options: options,
            cancelToken: cancellationToken
        );
        
        return BuildPoll(pollTweet!.ID!);
    }

    public override Task<List<PollResult>> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<PollResult>
        {
            new(5, 35),
            new(6, 42),
            new(7, 14),
            new(8, 23),
        });
    }
}
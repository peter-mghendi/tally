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
        var random = new Random();
        var options = 4;
        var start = ((channelPoll.Poll.Id - 1) * options) + 1;
        var optionVotes = Enumerable.Range(start, options)
            .Select<int, PollResult>(i => new(i, random.Next(20, 50)));
        return Task.FromResult(optionVotes.ToList());
    }
}
using LinqToTwitter;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Channels;

public class TwitterChannel : Channel
{
    private readonly TwitterContext _context;
    private readonly TallyContext _tallyContext;

    public TwitterChannel(TwitterContext context, TallyContext tallyContext, ILogger<TwitterChannel> logger)
    {
        _context = context;
        _tallyContext = tallyContext;
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

    public override async Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll,
        CancellationToken cancellationToken = default)
    {
        var items = await _tallyContext.CachedVotes.Include(cv => cv.Option)
            .Where(cv => cv.Channel == PollChannel.Twitter && cv.Poll.Id == channelPoll.Poll.Id)
            .Select(cv => new {Result = new PollResult(cv.Option.Id, cv.Count), Refreshed = cv.LastRefreshedAt})
            .ToListAsync(cancellationToken);
        return CachedResult(items.Select(i => i.Result).ToList(), items.Min(i => i.Refreshed));
    }
}
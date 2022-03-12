using LinqToTwitter;
using Web.Models;

namespace Web.Channels;

public class TwitterChannel
{
    private readonly TwitterContext _context;
    private readonly ILogger<TwitterChannel> _logger;

    public TwitterChannel(TwitterContext context, ILogger<TwitterChannel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options,
        CancellationToken cancellationToken = default)
    {
        var pollTweet = await _context.TweetPollAsync(
            text: question,
            duration: (int) TimeSpan.FromDays(7).TotalMinutes,
            options: options,
            cancelToken: cancellationToken
        );
        
        var telegramPoll = new ChannelPoll
        {
            Channel = PollChannel.Twitter,
            Identifier = pollTweet?.Attachments?.PollIds?[0] ?? string.Empty
        };

        return telegramPoll;
    }
}
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tweetinvi;
using Web.Data;
using Web.Models;
using static System.Text.Json.JsonSerializer;

namespace Web.Services;

public class TwitterUpdateService : IHostedService, IDisposable
{
    private readonly ILogger<TwitterUpdateService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer? _timer;

    public TwitterUpdateService(ILogger<TwitterUpdateService> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Twitter update service.");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        return Task.CompletedTask;
    }
    
    private async void DoWork(object? state)
    {
        var random = new Random();
        
        using var scope = _serviceScopeFactory.CreateScope();
        var tallyContext = scope.ServiceProvider.GetRequiredService<TallyContext>();
        var twitterClient = scope.ServiceProvider.GetRequiredService<TwitterClient>();
        
        var channelPolls = await tallyContext.ChannelPolls
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Options)
            .Where(p => p.Channel == PollChannel.Twitter)
            .ToListAsync();

        _logger.LogInformation("Found {Count} polls with twitter channels.", channelPolls.Count);

        foreach (var channelPoll in channelPolls)
        {
            var poll = channelPoll.Poll;
            var tweet = await twitterClient.TweetsV2.GetTweetAsync(channelPoll.Identifier);
            var tweetPollOptions = tweet.Includes.Polls[0].PollOptions;

            var voteCounts = poll.Options.Select((t, i) => new CachedVote
            {
                Count = tweetPollOptions[i].Votes, Channel = PollChannel.Twitter, Option = t, Poll = poll,
            });

            var currentCache = tallyContext.CachedVotes.Where(cv => cv.Channel == PollChannel.Twitter && cv.Poll.Id == poll.Id);
            tallyContext.CachedVotes.RemoveRange(currentCache);
        
            await tallyContext.CachedVotes.AddRangeAsync(voteCounts);
        }
       
        await tallyContext.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Twitter update service.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
    
    public void Dispose() => _timer?.Dispose();
}
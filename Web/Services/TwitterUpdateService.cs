using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tweetinvi;
using Web.Channels;
using Web.Data;
using Web.Hubs;
using Web.Models;

namespace Web.Services;

public sealed class TwitterUpdateService : BackgroundService
{
    private readonly IHubContext<TallyHub, TallyHub.ITallyHubClient> _hubContext;
    private readonly ILogger<TwitterUpdateService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TwitterUpdateService(
        IHubContext<TallyHub, TallyHub.ITallyHubClient> hubContext,
        ILogger<TwitterUpdateService> logger,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _hubContext = hubContext;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Twitter update service.");
        
        // HACK: Run the first update operation manually, because the timer does not.
        await TryUpdateAsync(cancellationToken);
        
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));
        while (await timer.WaitForNextTickAsync(cancellationToken)) await TryUpdateAsync(cancellationToken);
        
        _logger.LogInformation("Stopping Twitter update service.");
    }
    
    private async Task TryUpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TallyContext>();
            var twitterClient = scope.ServiceProvider.GetRequiredService<TwitterClient>();
            var channel = scope.ServiceProvider.GetRequiredService<TwitterChannel>();

            var channelPolls = await context.ChannelPolls
                .Include(cp => cp.Poll)
                .ThenInclude(p => p.Options)
                .Where(p => p.Channel == PollChannel.Twitter && p.Poll.EndedAt == null)
                .ToListAsync(cancellationToken: cancellationToken);

            if (channelPolls.Count <= 0) return; // Nothing to see here.
            
            _logger.LogInformation("Found {Count} ongoing polls with twitter channels.", channelPolls.Count);

            var tweetIds = channelPolls.Select(p => p.PrimaryIdentifier).ToArray();
            var tweetResponse = await twitterClient.TweetsV2.GetTweetsAsync(tweetIds);
            var tweets = tweetResponse.Tweets;

            for (var i = 0; i < tweets.Length; i++)
            {
                var tweetPoll = tweetResponse.Includes.Polls[i];
                var tweetPollOptions = tweetPoll.PollOptions;
                var poll = channelPolls[i].Poll;

                var voteCounts = poll.Options.Select((option, index) => new CachedVote
                {
                    Count = tweetPollOptions[index].Votes, 
                    Channel = PollChannel.Twitter, 
                    Option = option, 
                    Poll = poll,
                });

                var currentCache =
                    context.CachedVotes.Where(cv => cv.Channel == PollChannel.Twitter && cv.Poll.Id == poll.Id);
                context.CachedVotes.RemoveRange(currentCache);

                await context.CachedVotes.AddRangeAsync(voteCounts, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
            foreach (var channelPoll in channelPolls)
            {
                var poll = channelPoll.Poll;
                await _hubContext.Clients.Group(poll.Id.ToString())
                    .UpdateResult(nameof(PollChannel.Twitter), await channel.CountVotesAsync(channelPoll, cancellationToken));
            }
        }
        catch (Exception exception)
        {
            _logger.LogInformation("Twitter update failed at {DateTime}: {Message}.", DateTime.Now, exception.Message);
        }
    }
}
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Tweetinvi;
using Web.Channels;
using Web.Data;
using Web.Hubs;
using Web.Models;

namespace Web.Services;

public sealed class TwitterUpdateService : IHostedService, IDisposable
{
    private readonly IHubContext<TallyHub, TallyHub.ITallyHubClient> _hubContext;
    private readonly ILogger<TwitterUpdateService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer? _timer;

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

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting Twitter update service.");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            await using var tallyContext = scope.ServiceProvider.GetRequiredService<TallyContext>();
            var twitterClient = scope.ServiceProvider.GetRequiredService<TwitterClient>();
            var channel = scope.ServiceProvider.GetRequiredService<TwitterChannel>();

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

                var voteCounts = poll.Options.Select((option, index) => new CachedVote
                {
                    Count = tweetPollOptions[index].Votes, 
                    Channel = PollChannel.Twitter, 
                    Option = option, 
                    Poll = poll,
                });

                var currentCache =
                    tallyContext.CachedVotes.Where(cv => cv.Channel == PollChannel.Twitter && cv.Poll.Id == poll.Id);
                tallyContext.CachedVotes.RemoveRange(currentCache);

                await tallyContext.CachedVotes.AddRangeAsync(voteCounts);
            }

            await tallyContext.SaveChangesAsync();
            foreach (var channelPoll in channelPolls)
            {
                var poll = channelPoll.Poll;
                await _hubContext.Clients.Group(poll.Id.ToString())
                    .UpdateResult(nameof(PollChannel.Twitter), await channel.CountVotesAsync(channelPoll));
            }
        }
        catch (Exception)
        {
            _logger.LogInformation("Twitter update failed at {DateTime}.", DateTime.Now);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Twitter update service.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
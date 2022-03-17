using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

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

        var pollsWithTwitter = from channelPoll in tallyContext.ChannelPolls where channelPoll.Channel == PollChannel.Twitter
                select channelPoll.Poll.Id;
        
        _logger.LogInformation("Found {Count} polls with twitter channels.", pollsWithTwitter.Count());

        foreach (var id in pollsWithTwitter)
        {
            var poll = await tallyContext.Polls.Include(p => p.Options).SingleAsync(p => p.Id == id);
            var options = poll.Options;

            var voteCounts = options.Select(option => new CachedVote
            {
                Count = random.Next(20, 50),
                Channel = PollChannel.Twitter,
                Option = option,
                Poll = poll,
            });

            var currentCache = tallyContext.CachedVotes.Where(cv => cv.Channel == PollChannel.Twitter && cv.Poll.Id == id);
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
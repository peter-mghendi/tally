using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.DiscussionComment;
using Web.Channels;
using Web.Data;
using Web.Hubs;
using Web.Models;

namespace Web.Services;

public sealed class GitHubUpdateService : WebhookEventProcessor
{
    private readonly IHubContext<TallyHub, TallyHub.ITallyHubClient> _hubContext;
    private readonly ILogger<GitHubUpdateService> _logger;
    private readonly IServiceProvider _services;

    public GitHubUpdateService(
        IHubContext<TallyHub, TallyHub.ITallyHubClient> hubContext,
        ILogger<GitHubUpdateService> logger,
        IServiceProvider services
    )
    {
        _hubContext = hubContext;
        _logger = logger;
        _services = services;
    }

    protected override Task ProcessPingWebhookAsync(WebhookHeaders headers, PingEvent pingEvent)
    {
        _logger.LogInformation("Acknowledged ping event for hook {Hook}: {Zen}", pingEvent.Hook.Id, pingEvent.Zen);
        return base.ProcessPingWebhookAsync(headers, pingEvent);
    }

    protected override async Task ProcessDiscussionCommentWebhookAsync(
        WebhookHeaders headers,
        DiscussionCommentEvent discussionCommentEvent,
        DiscussionCommentAction action
    )
    {
        const string created = nameof(created);
        const string edited = nameof(edited);
        const string deleted = nameof(deleted);

        using var scope = _services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<TallyContext>();
        var channel = scope.ServiceProvider.GetRequiredService<GitHubChannel>();

        var identifier = discussionCommentEvent.Discussion.Number.ToString();
        var channelPoll = await context.ChannelPolls
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Options)
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.LiveVotes)
            .SingleAsync(cp => cp.Identifier == identifier && cp.Channel == PollChannel.GitHub);
        var poll = channelPoll.Poll;
        var userIdentifier = discussionCommentEvent.Comment.User.Id.ToString();

        _logger.LogInformation("Received GitHub vote for poll {Poll} with action {Action}", poll.Id, discussionCommentEvent.Action);

        var optionIndex = -1;
        if (discussionCommentEvent.Action is created or edited)
        {
            var body = discussionCommentEvent.Comment.Body.Trim();
            if (!int.TryParse(discussionCommentEvent.Comment.Body, out optionIndex))
            {
                _logger.LogInformation("Discarding vote: Vote body {Body} not a valid index", body);
                return;
            }

            if (optionIndex < 0 || optionIndex >= poll.Options.Count)
            {
                _logger.LogInformation("Discarding vote: Index {Index} outside required range.", optionIndex);
                return;
            }
        }

        if (discussionCommentEvent.Action is edited or deleted)
        {
            poll.LiveVotes.Remove(poll.LiveVotes
                .Single(v => v.Channel == PollChannel.GitHub && v.UserIdentifier == userIdentifier));
            await context.SaveChangesAsync();
        }

        if (discussionCommentEvent.Action is created or edited)
        {
            if (await context.LiveVotes.AnyAsync(lv =>
                    lv.UserIdentifier == userIdentifier &&
                    lv.Channel == PollChannel.GitHub &&
                    lv.Poll.Id == poll.Id))
            {
                _logger.LogInformation("Discarding vote: Duplicate vote from user {User} for poll {Poll}.",
                    userIdentifier, poll.Id);
                return;
            }

            poll.LiveVotes.Add(new LiveVote
            {
                Channel = PollChannel.GitHub,
                Option = poll.Options[optionIndex],
                UserIdentifier = userIdentifier
            });
        }

        await context.SaveChangesAsync();
        await base.ProcessDiscussionCommentWebhookAsync(headers, discussionCommentEvent, action);
        await _hubContext.Clients.Group(poll.Id.ToString())
            .UpdateResult(nameof(PollChannel.GitHub), await channel.CountVotesAsync(channelPoll));
    }
}
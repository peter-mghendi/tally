using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.DiscussionComment;
using Tally.Web.Channels;
using Tally.Web.Data;
using Tally.Web.Hubs;
using Tally.Web.Models;

namespace Tally.Web.Services;

public sealed class GitHubUpdateService : WebhookEventProcessor
{
    private readonly IHubContext<TallyHub, TallyHub.IClient> _hubContext;
    private readonly ILogger<GitHubUpdateService> _logger;
    private readonly IServiceProvider _services;

    public GitHubUpdateService(
        IHubContext<TallyHub, TallyHub.IClient> hubContext,
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

        var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TallyContext>();
        var channel = scope.ServiceProvider.GetRequiredService<GitHubChannel>();

        var identifier = discussionCommentEvent.Discussion.NodeId;
        var channelPoll = await context.ChannelPolls
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.Options)
            .Include(cp => cp.Poll)
            .ThenInclude(p => p.LiveVotes)
            .SingleAsync(cp => cp.PrimaryIdentifier == identifier && cp.Channel == PollChannel.GitHub);
        var poll = channelPoll.Poll;

        if (poll.EndedAt is not null)
        {
            _logger.LogInformation("Discarding GitHub vote for poll {Poll}: Poll has concluded.", poll.Id);
        }
        
        var userIdentifier = discussionCommentEvent.Comment.User.Id.ToString();

        _logger.LogInformation("Received GitHub vote for poll {Poll} with action {Action}", poll.Id, discussionCommentEvent.Action);

        var optionIndex = -1;
        if (discussionCommentEvent.Action is created or edited)
        {
            var body = discussionCommentEvent.Comment.Body.Trim();
            if (!int.TryParse(discussionCommentEvent.Comment.Body, out optionIndex))
            {
                _logger.LogInformation("Discarding GitHub vote for poll {Poll}: Vote body {Body} not a valid index", poll.Id, body);
                return;
            }

            if (optionIndex < 0 || optionIndex >= poll.Options.Count)
            {
                _logger.LogInformation("Discarding GitHub vote for poll {Poll}: Index {Index} outside required range.", poll.Id, optionIndex);
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
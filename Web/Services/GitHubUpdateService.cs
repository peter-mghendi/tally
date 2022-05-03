using Microsoft.EntityFrameworkCore;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.DiscussionComment;
using Web.Data;
using Web.Models;

namespace Web.Services;

public class GitHubUpdateService : WebhookEventProcessor
{
    private readonly ILogger<GitHubUpdateService> _logger;

    public GitHubUpdateService(ILogger<GitHubUpdateService> logger) => _logger = logger;

    protected override Task ProcessPingWebhookAsync(WebhookHeaders headers, PingEvent pingEvent)
    {
        _logger.LogInformation("Acknowledged ping event for hook {Hook}: {Zen}", pingEvent.Hook.Id, pingEvent.Zen);
        return base.ProcessPingWebhookAsync(headers, pingEvent);
    }
}
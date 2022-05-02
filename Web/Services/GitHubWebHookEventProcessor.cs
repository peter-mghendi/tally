using Octokit.Webhooks;
using Octokit.Webhooks.Events;

namespace Web.Services;

public class GitHubWebHookEventProcessor : WebhookEventProcessor
{
    private readonly ILogger<GitHubWebHookEventProcessor> _logger;

    public GitHubWebHookEventProcessor(ILogger<GitHubWebHookEventProcessor> logger) => _logger = logger;
    protected override Task ProcessPingWebhookAsync(WebhookHeaders headers, PingEvent pingEvent)
    {
        _logger.LogInformation("Acknowledged ping event for hook {Hook}: {Zen}", pingEvent.Hook.Id, pingEvent.Zen);
        return base.ProcessPingWebhookAsync(headers, pingEvent);
    }
}
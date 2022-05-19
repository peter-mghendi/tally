using Octokit;
using Web.Models.Configuration;

namespace Web.Services;

public sealed class GitHubWebHookService : IHostedService
{
    private readonly ILogger<TelegramWebhookService> _logger;
    private readonly IServiceProvider _services;
    private readonly BaseConfiguration _baseConfig;
    private readonly GitHubBotConfiguration _githubBotConfig;

    private RepositoryHook? _hook;

    public GitHubWebHookService(ILogger<TelegramWebhookService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _services = serviceProvider;    
        _baseConfig = configuration.GetRequiredSection(nameof(BaseConfiguration)).Get<BaseConfiguration>();
        _githubBotConfig = configuration.GetRequiredSection(nameof(GitHubBotConfiguration)).Get<GitHubBotConfiguration>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var scope = _services.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<GitHubClient>();

        // Configure custom endpoint per Telegram API recommendations:
        // REF: https://core.telegram.org/bots/api#setwebhook
        var webhookAddress = @$"{_baseConfig.HostAddress}/webhooks/github";
        _logger.LogInformation("Setting GitHub webhook: {webhookAddress}", webhookAddress);
        
        var hook = new NewRepositoryWebHook("web", new Dictionary<string, string>(), webhookAddress)
        {
            ContentType = WebHookContentType.Json,
            Events = new []{ "discussion_comment" },
            Secret = _githubBotConfig.WebHookSecret,
            Active = true,
            InsecureSsl = false            
        };
        
        _hook = await client.Repository.Hooks.Create(_githubBotConfig.RepositoryOwner, _githubBotConfig.RepositoryName, hook);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var scope = _services.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<GitHubClient>();

        // Remove webhook upon app shutdown
        if (_hook is not null)
        {
            _logger.LogInformation("Removing GitHub webhook");
            await client.Repository.Hooks.Delete(_githubBotConfig.RepositoryOwner, _githubBotConfig.RepositoryName,
                _hook.Id);
            _hook = null;
        }
    }
}
using LinqToTwitter;
using LinqToTwitter.OAuth;
using Octokit;
using Octokit.Webhooks;
using Telegram.Bot;
using Tweetinvi;
using Tweetinvi.Models;
using Web.Channels;
using Web.Models.Configuration;
using Connection = Octokit.GraphQL.Connection;

namespace Web.Services;

public static class ServicesConfiguration
{
    public static void AddTelegram(this IServiceCollection services, TelegramBotConfiguration telegramBotConfig)
    {
        services.AddHttpClient("tgwebhook")
            .AddTypedClient<ITelegramBotClient>(client => new TelegramBotClient(telegramBotConfig.BotToken, client));
        services.AddHostedService<TelegramWebhookService>();
        services.AddScoped<TelegramUpdateService>();
        services.AddScoped<TelegramChannel>();
    }

    public static void AddTwitter(this IServiceCollection services, TwitterBotConfiguration twitterBotConfig)
    {
        // LinqToTwitter - Publisher
        services.AddScoped(_ => new TwitterContext(new SingleUserAuthorizer
        {
            CredentialStore = new SingleUserInMemoryCredentialStore
            {
                ConsumerKey = twitterBotConfig.ConsumerKey,
                ConsumerSecret = twitterBotConfig.ConsumerSecret,
                AccessToken = twitterBotConfig.AccessToken,
                AccessTokenSecret = twitterBotConfig.AccessTokenSecret
            }
        }));

        // Tweetinvi - Consumer
        services.AddScoped(_ => new TwitterClient(new TwitterCredentials(
            twitterBotConfig.ConsumerKey,
            twitterBotConfig.ConsumerSecret,
            twitterBotConfig.AccessToken,
            twitterBotConfig.AccessTokenSecret
        )));

        services.AddHostedService<TwitterUpdateService>();
        services.AddScoped<TwitterChannel>();
    }

    public static void AddGitHub(this IServiceCollection services, GitHubBotConfiguration gitHubBotConfig)
    {
        // GraphQL
        services.AddScoped(_ => new Connection(new("Tally", "1.0"), gitHubBotConfig.Token));

        // REST
        services.AddScoped(_ => new GitHubClient(new ProductHeaderValue("Tally", "1.0"))
        {
            Credentials = new Credentials(gitHubBotConfig.Token)
        });
        services.AddSingleton<WebhookEventProcessor, GitHubUpdateService>();
        services.AddHostedService<GitHubWebHookService>();
        services.AddScoped<GitHubChannel>();
    }

    public static void AddDiscord(this IServiceCollection services, DiscordBotConfiguration discordBotConfig)
    {
        services.AddSingleton(_ => discordBotConfig);
        services.AddScoped<DiscordUpdateService>();
        services.AddSingleton<DiscordGatewayService>();
        services.AddHostedService(provider => provider.GetRequiredService<DiscordGatewayService>());
        services.AddScoped<DiscordChannel>();
    }
}
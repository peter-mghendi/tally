using LinqToTwitter;
using LinqToTwitter.OAuth;
using Octokit;
using Octokit.Webhooks;
using Tally.Web.Channels;
using Tally.Web.Models.Configuration;
using Telegram.Bot;
using Tweetinvi;
using Tweetinvi.Models;
using Connection = Octokit.GraphQL.Connection;

namespace Tally.Web.Services;

public static class ServicesConfiguration
{
    public static void AddTelegram(this IServiceCollection services, TelegramBotConfiguration telegramBotConfig)
    {
        services.AddHttpClient("TelegramWebhook")
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
        services.AddSingleton<DiscordAdapter>();
        services.AddScoped<DiscordUpdateService>();
        services.AddHostedService<DiscordGatewayService>();
        services.AddScoped<DiscordChannel>();
    }

    public static void AddWeb(this IServiceCollection services)
    {
        services.AddScoped<WebChannel>();
    }
}
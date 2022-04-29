namespace Web.Models.Configuration;

public class TwitterBotConfiguration
{
    public string ConsumerKey { get; init; } = null!;
    public string ConsumerSecret { get; init; } = null!;
    public string AccessToken { get; init; } = null!;
    public string AccessTokenSecret { get; init; } = null!;
}
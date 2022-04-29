namespace Web.Models.Configuration;

public class GitHubBotConfiguration
{
    public string CategoryId { get; init; } = null!;
    public string Token { get; init; } = null!;
    public string RepositoryId { get; init; } = null!;
}
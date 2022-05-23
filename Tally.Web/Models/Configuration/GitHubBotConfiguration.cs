namespace Tally.Web.Models.Configuration;

public class GitHubBotConfiguration
{
    public string CategoryId { get; init; } = null!;
    public string Token { get; init; } = null!;
    public string RepositoryId { get; init; } = null!;
    public string RepositoryOwner { get; init; } = null!;
    public string RepositoryName { get; init; } = null!;
    public string WebHookSecret { get; init; } = null!;
}
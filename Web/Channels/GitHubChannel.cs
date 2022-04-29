using System.Text;
using Microsoft.EntityFrameworkCore;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using Web.Data;
using Web.Models;
using Web.Models.Configuration;

namespace Web.Channels;

public class GitHubChannel : Channel
{
    private readonly Connection _connection;
    private readonly string _categoryId;
    private readonly string _repositoryId;
    private readonly TallyContext _tallyContext;
    
    public GitHubChannel(Connection connection, IConfiguration configuration, TallyContext tallyContext)
    {
        var gitHubBotConfiguration = configuration.GetRequiredSection(nameof(GitHubBotConfiguration))
                .Get<GitHubBotConfiguration>();
        
        _connection = connection;
        _categoryId = gitHubBotConfiguration.CategoryId;
        _repositoryId = gitHubBotConfiguration.RepositoryId;
        _tallyContext = tallyContext;
    }
    
    public override PollChannel PollChannel => PollChannel.GitHub;
    
    public override async Task<ChannelPoll> CreatePollAsync(string question, IEnumerable<string> options, CancellationToken cancellationToken = default)
    {
        var bodyBuilder = new StringBuilder($"{question}\n\nReply with one of the following to vote:");
        var enumerable = options as string[] ?? options.ToArray();
        for (var i = 0; i < enumerable.Length; i++)
        {
            bodyBuilder.Append($"{i}: {enumerable.ElementAt(i)}\n");
        }
        
        var mutation = new Mutation(
            ).CreateDiscussion(new CreateDiscussionInput()
            {
                Title = question, 
                Body = bodyBuilder.ToString(),
                CategoryId = new ID(_categoryId),
                RepositoryId = new ID(_repositoryId)
            })
            .Select(payload => new { payload.Discussion.Number })
            .Compile();

        var result = await _connection.Run(mutation, cancellationToken: cancellationToken);
        return BuildPoll(result.Number.ToString());
    }

    public override async Task<ChannelResult> CountVotesAsync(ChannelPoll channelPoll, CancellationToken cancellationToken = default)
    {
        var query = from option in _tallyContext.Options where option.Poll.Id == channelPoll.Poll.Id
            select new PollResult(option.Id, option.LiveVotes.Count(lv => lv.Channel == PollChannel.GitHub));
        return LiveResult(await query.ToListAsync(cancellationToken));
    }
}
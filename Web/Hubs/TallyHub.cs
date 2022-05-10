using Microsoft.AspNetCore.SignalR;
using Web.Models;

namespace Web.Hubs;

public class TallyHub : Hub<TallyHub.ITallyHubClient>
{
    public interface ITallyHubClient
    {
        public Task AcknowledgeSubscription(int pollId);
        public Task UpdateResults(Dictionary<string, ChannelResult> results);
        public Task UpdateResult(string channel, ChannelResult results);
    }

    public async Task Subscribe(int pollId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, pollId.ToString());
        await Clients.Caller.AcknowledgeSubscription(pollId);
    }
}
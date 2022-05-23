using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Channels;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Pages.Polls;

[Authorize]
public class Details : PageModel
{
    private readonly TallyContext _context;
    private readonly ChannelWrapper _channels;

    [BindProperty(SupportsGet = true)] 
    public int Id { get; set; }
    
    [BindProperty] 
    public Poll Poll { get; set; }
    
    [BindProperty] 
    public Dictionary<string, ChannelResult> Results { get; set; }

    public Details(TallyContext context, ChannelWrapper channels)
    {
        _context = context;
        _channels = channels;

        Poll = new Poll();
        Results = new Dictionary<string, ChannelResult>();
    }

    public async Task OnGetAsync()
    {
        Poll = await _context.Polls
            .Include(p => p.ChannelPolls)
            .Include(p => p.Options)
            .SingleAsync(p => p.Id == Id);

        Results = new Dictionary<string, ChannelResult>();

        foreach (var channelPoll in Poll.ChannelPolls)
        {
            var pollChannel = channelPoll.Channel;
            var channel = _channels.Resolve(pollChannel);
            Results.Add(pollChannel.ToString(), await  channel.CountVotesAsync(channelPoll));
        }
    }
}
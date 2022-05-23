using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Channels;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Pages.Polls;

[Authorize]
public class Delete : PageModel
{
    private readonly ChannelWrapper _channels;
    private readonly ILogger<Delete> _logger;
    private readonly TallyContext _context;

    public Delete(ChannelWrapper channels, ILogger<Delete> logger, TallyContext context)
    {
        _channels = channels;
        _logger = logger;
        _context = context;

        Poll = new Poll();
    }
    
    [BindProperty(SupportsGet = true)] 
    public int Id { get; set; }
    
    [BindProperty]
    public Poll Poll { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        Poll = await _context.Polls.Include(p => p.ChannelPolls).SingleAsync(p => p.Id == Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Poll = await _context.Polls.Include(p => p.ChannelPolls).SingleAsync(p => p.Id == Id);
        _logger.LogInformation("Deleting poll with id: {Id}", Id);
        
        foreach (var channelPoll in Poll.ChannelPolls)
        {
            var channel = _channels.Resolve(channelPoll.Channel);
            await channel.DeletePollAsync(channelPoll);
        }

        _context.Polls.Remove(Poll);
        await _context.SaveChangesAsync();
        
        // TODO: Flash message
        return RedirectToPage("./Index");
    }
}
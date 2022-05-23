using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Channels;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Pages.Polls;

[Authorize]
public class Conclude : PageModel
{
    private ChannelWrapper _channels;
    private ILogger<Conclude> _logger;
    private readonly TallyContext _context;

    public Conclude(ChannelWrapper channels, ILogger<Conclude> logger, TallyContext context)
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
        
        if (Poll.EndedAt is not null)
        {
            // TODO: Flash message
            return RedirectToPage("./Details", new { Id });
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        Poll = await _context.Polls.Include(p => p.ChannelPolls).SingleAsync(p => p.Id == Id);
        _logger.LogInformation("Concluding poll with id: {Id}", Id);

        foreach (var channelPoll in Poll.ChannelPolls)
        {
            var channel = _channels.Resolve(channelPoll.Channel);
            await channel.ConcludePollAsync(channelPoll);
        }
        
        Poll.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        // TODO: Flash message
        return RedirectToPage("./Details", new { Id });
    }
}
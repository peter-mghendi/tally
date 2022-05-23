using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Pages.Polls;

public class Vote : PageModel
{
    private readonly TallyContext _context;
    private readonly UserManager<User> _manager;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    
    [BindProperty] 
    public Poll Poll { get; set; }
    [BindProperty] 
    public int? Chosen { get; set; }
    
    [BindProperty] 
    public double[] Results { get; set; }

    public Vote(TallyContext context, UserManager<User> manager)
    {
        _context = context;
        _manager = manager;

        Poll = new Poll();
        Results = new[] { 0d, 0d, 0d, 0d };
    }
    
    public async Task OnGetAsync()
    {
        Poll = await _context.Polls.Include(p => p.Options)
            .ThenInclude(o => o.LiveVotes)
            .Include(p => p.ChannelPolls)
            .SingleAsync(p => p.Id == Id);

        var user = await _manager.GetUserAsync(HttpContext.User);
        Chosen = Poll.LiveVotes.SingleOrDefault(p => p.UserIdentifier == user.Id && p.Channel == PollChannel.Web)?.OptionId;

        var votes = Poll.Options.Select(option => (double)option.LiveVotes.Count).ToList();
        var sum = votes.Sum();

        if (sum > 0) Results = votes.Select(v => (v / sum) * 100).ToArray();
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly TallyContext _context;

    [BindProperty] 
    public List<Poll> Polls { get; set; }
    
    public IndexModel(ILogger<IndexModel> logger, TallyContext context)
    {
        _logger = logger;
        _context = context;

        Polls = new List<Poll>();
    }

    public async Task<IActionResult> OnGet()
    {
        var query = from poll in _context.Polls.Include(p => p.ChannelPolls)
            .Include(p => p.Creator) 
            orderby poll.StartedAt descending 
            select poll;
        Polls = await query.ToListAsync();

        return Page();
    }
}
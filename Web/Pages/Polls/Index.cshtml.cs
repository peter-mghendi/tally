using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Pages.Polls;

public class Index : PageModel
{
    private readonly TallyContext _context;

    public IList<Poll> Polls { get; set; }

    public Index(TallyContext context)
    {
        _context = context;
    }

    public async Task OnGetAsync()
    {
        Polls = await _context.Polls
            .Include(poll => poll.Creator)
            .Include(poll => poll.ChannelPolls).ToListAsync();
    }
}
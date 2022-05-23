using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Tally.Web.Data;
using Tally.Web.Models;

namespace Tally.Web.Pages.Polls;

[Authorize]
public class Index : PageModel
{
    private readonly TallyContext _context;
    private readonly UserManager<User> _userManager;

    [BindProperty]
    public IList<Poll> Polls { get; set; }

    public Index(TallyContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;

        Polls = new List<Poll>();
    }

    public async Task OnGetAsync()
    {
        
        Polls = await _context.Polls
            .Include(poll => poll.Creator)
            .Include(poll => poll.ChannelPolls)
            .Where(p => p.Creator.Id == _userManager.GetUserId(User))
            .ToListAsync();
    }
}
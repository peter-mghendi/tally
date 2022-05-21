using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Pages.Polls;

public class Vote : PageModel
{
    private readonly TallyContext _context;
    
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }
    
    [BindProperty] 
    public Poll Poll { get; set; }

    public Vote(TallyContext context)
    {
        _context = context;

        Poll = new Poll();
    }
    
    public async Task OnGetAsync()
    {
        Poll = await _context.Polls.Include(p => p.Options)
            .Include(p => p.ChannelPolls)
            .SingleAsync(p => p.Id == Id);
    }
}
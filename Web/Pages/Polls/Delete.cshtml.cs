using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Pages.Polls;

[Authorize]
public class Delete : PageModel
{
    private readonly ILogger<Delete> _logger;
    private readonly TallyContext _context;

    public Delete(ILogger<Delete> logger, TallyContext context)
    {
        _logger = logger;
        _context = context;
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
        Poll = await _context.Polls.SingleAsync(p => p.Id == Id);
        _logger.LogInformation("Deleting poll with id: {Id}", Id);
        return RedirectToPage("./Index");
    }
}
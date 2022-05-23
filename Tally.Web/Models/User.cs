using Microsoft.AspNetCore.Identity;

namespace Tally.Web.Models;

public class User : IdentityUser
{
    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
}
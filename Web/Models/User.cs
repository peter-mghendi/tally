using Microsoft.AspNetCore.Identity;

namespace Web.Models;

public class User : IdentityUser
{
    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
}
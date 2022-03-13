namespace Web.Models;

public class Option
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public Poll Poll { get; set; } = null!;
    public IList<Vote> Votes { get; set; } = new List<Vote>();
}
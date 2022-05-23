namespace Tally.Web.Models;

public record class ChannelResult(List<PollResult> Results, bool Live, DateTime? LastRefreshed = default);
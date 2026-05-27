namespace SportsLeague.Domain.Entities;
public class MatchLineup : AuditBase
{
    public int MatchId { get; set; }
    public Match Match { get; set; } = null!;
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public bool IsStarter { get; set; }
    public string Position { get; set; } = string.Empty;
}
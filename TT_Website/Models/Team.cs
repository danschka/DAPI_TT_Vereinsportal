namespace TT_Website.Models;

public class Team
{
    public int Id { get; set; }

    // Wird manuell vom Admin gepflegt
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string? MyTischtennisLeagueUrl { get; set; }
    public string? MyTischtennisStatisticsUrl { get; set; }
    public string? MyTischtennisScheduleUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Wird später automatisch aus myTischtennis gelesen
    public string? League { get; set; }
    public string? Season { get; set; }

    public string? TableDataJson { get; set; }
    public string? ScheduleDataJson { get; set; }
    public string? StatisticsDataJson { get; set; }

    public DateTime? LastSyncedAt { get; set; }

    public List<TeamRound> Rounds { get; set; } = new();
}

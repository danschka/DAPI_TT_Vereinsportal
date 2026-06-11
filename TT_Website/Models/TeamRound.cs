namespace TT_Website.Models;

public class TeamRound
{
    public int Id { get; set; }

    public int TeamId { get; set; }
    public Team? Team { get; set; }

    public string RoundName { get; set; } = "";
    public string? MyTischtennisLeagueUrl { get; set; }
    public string? MyTischtennisStatisticsUrl { get; set; }
    public string? MyTischtennisScheduleUrl { get; set; }

    public string? League { get; set; }
    public string? Season { get; set; }

    public string? TableDataJson { get; set; }
    public string? ScheduleDataJson { get; set; }
    public string? StatisticsDataJson { get; set; }

    public DateTime? LastSyncedAt { get; set; }
}

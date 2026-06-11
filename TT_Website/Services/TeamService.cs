using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public record TeamSyncResult(bool Success, string Message);

public class TeamService
{
    private readonly AppDbContext _context;
    private readonly MyTischtennisImportService _importService;

    public TeamService(
        AppDbContext context,
        MyTischtennisImportService importService)
    {
        _context = context;
        _importService = importService;
    }

    public async Task<List<Team>> GetAllAsync()
    {
        return await _context.Teams
            .Include(t => t.Rounds)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Team>> GetActiveAsync()
    {
        return await _context.Teams
            .Include(t => t.Rounds)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _context.Teams
            .Include(t => t.Rounds)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TeamSyncResult> AddAsync(Team team)
    {
        team.IsActive = true;

        var importResult = await ImportTeamDataAsync(team);

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return importResult;
    }

    public async Task SaveRoundsAsync(
        int teamId,
        IReadOnlyDictionary<string, string?> roundUrls,
        IReadOnlyDictionary<string, string?>? roundStatisticsUrls = null,
        IReadOnlyDictionary<string, string?>? roundScheduleUrls = null)
    {
        var team = await _context.Teams
            .Include(t => t.Rounds)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team is null)
            return;

        foreach (var (roundName, url) in roundUrls)
        {
            var round = team.Rounds.FirstOrDefault(x =>
                x.RoundName.Equals(roundName, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(url))
            {
                if (round is not null)
                {
                    _context.TeamRounds.Remove(round);
                }

                continue;
            }

            if (round is null)
            {
                round = new TeamRound
                {
                    TeamId = teamId,
                    RoundName = roundName
                };

                _context.TeamRounds.Add(round);
            }

            round.MyTischtennisLeagueUrl = url.Trim();
            round.MyTischtennisStatisticsUrl =
                roundStatisticsUrls is not null &&
                roundStatisticsUrls.TryGetValue(roundName, out var statisticsUrl) &&
                !string.IsNullOrWhiteSpace(statisticsUrl)
                    ? statisticsUrl.Trim()
                    : null;
            round.MyTischtennisScheduleUrl =
                roundScheduleUrls is not null &&
                roundScheduleUrls.TryGetValue(roundName, out var scheduleUrl) &&
                !string.IsNullOrWhiteSpace(scheduleUrl)
                    ? scheduleUrl.Trim()
                    : null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<TeamSyncResult> UpdateAsync(Team team)
    {
        var existingTeam = await _context.Teams.FindAsync(team.Id);

        if (existingTeam is null)
            return new TeamSyncResult(false, "Mannschaft wurde nicht gefunden.");

        existingTeam.Name = team.Name;
        existingTeam.Category = team.Category;
        existingTeam.IsActive = team.IsActive;
        existingTeam.MyTischtennisLeagueUrl = team.MyTischtennisLeagueUrl;
        existingTeam.MyTischtennisStatisticsUrl = team.MyTischtennisStatisticsUrl;
        existingTeam.MyTischtennisScheduleUrl = team.MyTischtennisScheduleUrl;

        var importResult = await ImportTeamDataAsync(existingTeam);

        await _context.SaveChangesAsync();

        return importResult;
    }

    public async Task<TeamSyncResult> SyncTeamAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team is null)
            return new TeamSyncResult(false, "Mannschaft wurde nicht gefunden.");

        if (string.IsNullOrWhiteSpace(team.MyTischtennisLeagueUrl))
            return new TeamSyncResult(false, "Für diese Mannschaft ist keine myTischtennis-URL hinterlegt.");

        var importResult = await ImportTeamDataAsync(team);

        if (importResult.Success)
            await _context.SaveChangesAsync();

        return importResult;
    }

    public async Task<TeamSyncResult> SyncTeamRoundAsync(int teamId, string roundName)
    {
        var round = await _context.TeamRounds
            .FirstOrDefaultAsync(x => x.TeamId == teamId && x.RoundName == roundName);

        if (round is null)
            return new TeamSyncResult(false, "Runde wurde nicht gefunden.");

        if (string.IsNullOrWhiteSpace(round.MyTischtennisLeagueUrl))
            return new TeamSyncResult(false, $"Für {GetRoundDisplayName(round.RoundName)} ist keine myTischtennis-URL hinterlegt.");

        try
        {
            var importData = await _importService.ImportAsync(
                round.MyTischtennisLeagueUrl,
                round.MyTischtennisStatisticsUrl,
                round.MyTischtennisScheduleUrl);

            round.League = importData.League;
            round.Season = importData.Season;
            round.TableDataJson = importData.TableDataJson;
            round.ScheduleDataJson = importData.ScheduleDataJson;
            round.StatisticsDataJson = importData.StatisticsDataJson;
            round.LastSyncedAt = importData.LastSyncedAt;

            await _context.SaveChangesAsync();

            return new TeamSyncResult(true, $"{GetRoundDisplayName(round.RoundName)} wurde aus myTischtennis aktualisiert.");
        }
        catch (InvalidOperationException ex)
        {
            return new TeamSyncResult(false, ex.Message);
        }
        catch (HttpRequestException)
        {
            return new TeamSyncResult(false, "myTischtennis konnte nicht erreicht werden.");
        }
        catch (TaskCanceledException)
        {
            return new TeamSyncResult(false, "Die Anfrage an myTischtennis hat zu lange gedauert.");
        }
        catch
        {
            return new TeamSyncResult(false, "Die Daten konnten nicht von myTischtennis geladen werden.");
        }
    }

    public async Task<TeamSyncResult> SyncTeamWithRoundsAsync(int teamId)
    {
        var team = await _context.Teams
            .Include(x => x.Rounds)
            .FirstOrDefaultAsync(x => x.Id == teamId);

        if (team is null)
            return new TeamSyncResult(false, "Mannschaft wurde nicht gefunden.");

        var results = new List<TeamSyncResult>();

        if (team.Rounds.Count > 0)
        {
            foreach (var round in team.Rounds)
            {
                results.Add(await SyncTeamRoundAsync(team.Id, round.RoundName));
            }
        }
        else
        {
            results.Add(await SyncTeamAsync(team.Id));
        }

        if (results.Any(x => x.Success))
            return new TeamSyncResult(true, "Daten wurden automatisch aktualisiert.");

        return results.FirstOrDefault() ?? new TeamSyncResult(false, "Keine Daten konnten aktualisiert werden.");
    }

    private async Task<TeamSyncResult> ImportTeamDataAsync(Team team)
    {
        if (string.IsNullOrWhiteSpace(team.MyTischtennisLeagueUrl))
            return new TeamSyncResult(false, "Für diese Mannschaft ist keine myTischtennis-URL hinterlegt.");

        try
        {
            await _importService.ImportAsync(team);
            return new TeamSyncResult(true, "Daten wurden aus myTischtennis aktualisiert.");
        }
        catch (InvalidOperationException ex)
        {
            return new TeamSyncResult(false, ex.Message);
        }
        catch (HttpRequestException)
        {
            return new TeamSyncResult(false, "myTischtennis konnte nicht erreicht werden.");
        }
        catch (TaskCanceledException)
        {
            return new TeamSyncResult(false, "Die Anfrage an myTischtennis hat zu lange gedauert.");
        }
        catch
        {
            return new TeamSyncResult(false, "Die Daten konnten nicht von myTischtennis geladen werden.");
        }
    }

    public async Task DeleteAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team is null)
            return;

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
    }

    private static string GetRoundDisplayName(string roundName)
    {
        return roundName == "Rueckrunde" ? "Rückrunde" : roundName;
    }
}

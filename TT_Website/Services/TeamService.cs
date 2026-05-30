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
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Team>> GetActiveAsync()
    {
        return await _context.Teams
            .Where(t => t.IsActive)
            .OrderBy(t => t.Category)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Team?> GetByIdAsync(int id)
    {
        return await _context.Teams
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

    public async Task<TeamSyncResult> UpdateAsync(Team team)
    {
        var existingTeam = await _context.Teams.FindAsync(team.Id);

        if (existingTeam is null)
            return new TeamSyncResult(false, "Mannschaft wurde nicht gefunden.");

        existingTeam.Name = team.Name;
        existingTeam.Category = team.Category;
        existingTeam.IsActive = team.IsActive;
        existingTeam.MyTischtennisLeagueUrl = team.MyTischtennisLeagueUrl;

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
}

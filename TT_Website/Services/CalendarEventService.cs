using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class CalendarEventService(AppDbContext context, IWebHostEnvironment environment)
{
    public async Task<List<CalendarEvent>> GetAllAsync()
    {
        var events = await context.CalendarEvents.AsNoTracking()
            .OrderBy(x => x.EventDate).ThenBy(x => x.Title).ToListAsync();
        ClearMissingImages(events);
        return events;
    }

    public async Task<List<CalendarEvent>> GetUpcomingAsync(DateTime from, DateTime through)
    {
        var start = from.Date;
        var end = through.Date.AddDays(1);
        var events = await context.CalendarEvents.AsNoTracking()
            .Where(x => x.EventDate >= start && x.EventDate < end)
            .OrderBy(x => x.EventDate).ThenBy(x => x.Title).ToListAsync();
        ClearMissingImages(events);
        return events;
    }

    public async Task<CalendarEvent?> GetNextAsync(DateTime from)
    {
        var result = await context.CalendarEvents.AsNoTracking()
            .Where(x => x.EventDate >= from)
            .OrderBy(x => x.EventDate).ThenBy(x => x.Title).FirstOrDefaultAsync();
        if (result is not null) ClearMissingImages([result]);
        return result;
    }

    public async Task AddAsync(CalendarEvent item)
    {
        context.CalendarEvents.Add(item);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CalendarEvent item)
    {
        var existing = await context.CalendarEvents.FindAsync(item.Id);
        if (existing is null) return;
        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.EventDate = item.EventDate;
        existing.ImagePath = item.ImagePath;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var existing = await context.CalendarEvents.FindAsync(id);
        if (existing is null) return;
        context.CalendarEvents.Remove(existing);
        await context.SaveChangesAsync();
    }

    private void ClearMissingImages(IEnumerable<CalendarEvent> events)
    {
        foreach (var item in events)
            if (WebFilePathValidator.GetExistingPath(environment, item.ImagePath) is null)
                item.ImagePath = null;
    }
}

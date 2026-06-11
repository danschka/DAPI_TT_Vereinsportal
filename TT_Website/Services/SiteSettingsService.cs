using Microsoft.EntityFrameworkCore;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public class SiteSettingsService
{
    public const string MemberApplicationRecipientEmail = "member_application_recipient_email";

    private readonly AppDbContext _context;

    public SiteSettingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        return await _context.SiteSettings
            .Where(x => x.Key == key)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        return await _context.SiteSettings
            .OrderBy(x => x.Key)
            .ToDictionaryAsync(x => x.Key, x => x.Value);
    }

    public async Task SetValueAsync(string key, string value)
    {
        var setting = await _context.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);

        if (setting is null)
        {
            _context.SiteSettings.Add(new SiteSetting
            {
                Key = key,
                Value = value.Trim(),
                UpdatedAt = DateTime.Now
            });
        }
        else
        {
            setting.Value = value.Trim();
            setting.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
    }
}

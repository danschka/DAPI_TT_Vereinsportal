using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TT_Website.Data;
using TT_Website.Models;

namespace TT_Website.Services;

public record MemberDevelopmentEntry(DateTime Date, int Value);

public class SiteSettingsService
{
    public const string MemberApplicationRecipientEmail = "member_application_recipient_email";
    public const string RankingUrl = "ranking_url";
    public const string MemberDevelopmentData = "member_development_data";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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

    public async Task<List<MemberDevelopmentEntry>> GetMemberDevelopmentAsync()
    {
        var value = await GetValueAsync(MemberDevelopmentData);

        if (string.IsNullOrWhiteSpace(value))
            return GetDefaultMemberDevelopment();

        try
        {
            return ParseMemberDevelopment(value);
        }
        catch (JsonException)
        {
            return GetDefaultMemberDevelopment();
        }
    }

    public async Task SetMemberDevelopmentAsync(IEnumerable<MemberDevelopmentEntry> entries)
    {
        var normalizedEntries = entries
            .Where(x => x.Date.Year >= 2014 && x.Date <= DateTime.Today.AddYears(1) && x.Value >= 0)
            .GroupBy(x => x.Date.Date)
            .Select(group => group.Last())
            .OrderBy(x => x.Date)
            .ToList();

        var json = JsonSerializer.Serialize(normalizedEntries, JsonOptions);
        await SetValueAsync(MemberDevelopmentData, json);
    }

    public static List<MemberDevelopmentEntry> GetDefaultMemberDevelopment()
    {
        return new List<MemberDevelopmentEntry>
        {
            new MemberDevelopmentEntry(new DateTime(2014, 1, 1), 120),
            new MemberDevelopmentEntry(new DateTime(2016, 1, 1), 132),
            new MemberDevelopmentEntry(new DateTime(2018, 1, 1), 145),
            new MemberDevelopmentEntry(new DateTime(2020, 1, 1), 152),
            new MemberDevelopmentEntry(new DateTime(2022, 1, 1), 160),
            new MemberDevelopmentEntry(new DateTime(2024, 1, 1), 166),
            new MemberDevelopmentEntry(new DateTime(DateTime.Today.Year, 1, 1), 170)
        }
        .GroupBy(x => x.Date.Date)
        .Select(group => group.Last())
        .OrderBy(x => x.Date)
        .ToList();
    }

    private static List<MemberDevelopmentEntry> ParseMemberDevelopment(string value)
    {
        using var document = JsonDocument.Parse(value);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return GetDefaultMemberDevelopment();

        var entries = new List<MemberDevelopmentEntry>();

        foreach (var element in document.RootElement.EnumerateArray())
        {
            var date = TryReadDate(element);
            var entryValue = TryReadInt(element, "value", "members");

            if (date is not null && entryValue is not null)
                entries.Add(new MemberDevelopmentEntry(date.Value.Date, entryValue.Value));
        }

        return entries.Count == 0
            ? GetDefaultMemberDevelopment()
            : entries
                .Where(x => x.Date.Year >= 2014 && x.Value >= 0)
                .GroupBy(x => x.Date.Date)
                .Select(group => group.Last())
                .OrderBy(x => x.Date)
                .ToList();
    }

    private static DateTime? TryReadDate(JsonElement element)
    {
        if (TryGetProperty(element, "date", out var dateProperty) &&
            dateProperty.ValueKind == JsonValueKind.String &&
            DateTime.TryParse(dateProperty.GetString(), out var parsedDate))
        {
            return parsedDate;
        }

        var year = TryReadInt(element, "year");
        return year is null ? null : new DateTime(year.Value, 1, 1);
    }

    private static int? TryReadInt(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!TryGetProperty(element, name, out var property))
                continue;

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
                return number;

            if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out number))
                return number;
        }

        return null;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement property)
    {
        foreach (var candidate in element.EnumerateObject())
        {
            if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                property = candidate.Value;
                return true;
            }
        }

        property = default;
        return false;
    }
}

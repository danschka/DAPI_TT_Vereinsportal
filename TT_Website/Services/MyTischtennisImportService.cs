using HtmlAgilityPack;
using System.Text.Json;
using System.Text.RegularExpressions;
using TT_Website.Models;

namespace TT_Website.Services;

public class MyTischtennisImportService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public MyTischtennisImportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task ImportAsync(Team team)
    {
        if (string.IsNullOrWhiteSpace(team.MyTischtennisLeagueUrl))
            return;

        if (!Uri.TryCreate(team.MyTischtennisLeagueUrl, UriKind.Absolute, out var url) ||
            (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Die hinterlegte myTischtennis-URL ist ungültig.");
        }

        var scheduleDoc = await LoadFirstDocumentAsync(CreateScheduleUrlCandidates(url));
        var statisticsDoc = await LoadFirstDocumentAsync(CreateStatisticsUrlCandidates(url));

        var scheduleText = CleanMultilineText(scheduleDoc.DocumentNode.InnerText);
        var statisticsText = CleanMultilineText(statisticsDoc.DocumentNode.InnerText);

        team.League = ExtractLeague(scheduleText, scheduleDoc) ?? ExtractLeague(statisticsText, statisticsDoc);
        team.Season = ExtractSeason(scheduleText) ?? ExtractSeason(statisticsText);

        var scheduleTable = FindBestTable(scheduleDoc, GetScheduleTableScore);
        var standingsTable = FindBestTable(scheduleDoc, table => GetStandingsTableScore(table, scheduleTable));
        var statisticsTable = FindBestTable(statisticsDoc, table => GetStatisticsTableScore(table, null, null));

        team.TableDataJson = SerializeRows(ReadTableRows(standingsTable));
        team.ScheduleDataJson = SerializeRows(ReadScheduleRows(scheduleTable));
        team.StatisticsDataJson = SerializeRows(ReadTableRows(statisticsTable));

        team.LastSyncedAt = DateTime.Now;
    }

    private async Task<HtmlDocument> LoadFirstDocumentAsync(IEnumerable<Uri> urls)
    {
        Exception? lastException = null;

        foreach (var url in urls.DistinctBy(uri => uri.AbsoluteUri))
        {
            try
            {
                return await LoadDocumentAsync(url);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }
            catch (TaskCanceledException ex)
            {
                lastException = ex;
            }
        }

        throw lastException ?? new InvalidOperationException("Keine passende myTischtennis-URL konnte geladen werden.");
    }

    private async Task<HtmlDocument> LoadDocumentAsync(Uri url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("TT_Website/1.0");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return doc;
    }

    private IEnumerable<Uri> CreateScheduleUrlCandidates(Uri source)
    {
        yield return ReplaceUrlPart(source, "spielerbilanzen", "spielplan");
        yield return ReplaceUrlPart(source, "spieler-bilanzen", "spielplan");
        yield return ReplaceUrlPart(source, "bilanzen", "spielplan");
        yield return source;
    }

    private IEnumerable<Uri> CreateStatisticsUrlCandidates(Uri source)
    {
        yield return source;
        yield return ReplaceUrlPart(source, "spielplan", "spielerbilanzen");
        yield return ReplaceUrlPart(source, "spielplan", "spieler-bilanzen");
        yield return ReplaceUrlPart(source, "spielplan", "bilanzen");
    }

    private Uri ReplaceUrlPart(Uri source, string oldValue, string newValue)
    {
        var url = source.AbsoluteUri;

        if (!url.Contains(oldValue, StringComparison.OrdinalIgnoreCase))
            return source;

        var replacedUrl = Regex.Replace(
            url,
            Regex.Escape(oldValue),
            newValue,
            RegexOptions.IgnoreCase);

        return new Uri(replacedUrl);
    }

    private string? ExtractLeague(string text, HtmlDocument doc)
    {
        var heading = doc.DocumentNode
            .SelectNodes("//h1|//h2|//h3|//*[contains(@class,'headline') or contains(@class,'title')]")
            ?.Select(node => CleanInlineText(node.InnerText))
            .FirstOrDefault(IsLeagueLine);

        if (!string.IsNullOrWhiteSpace(heading))
            return heading;

        var detailedLeagueMatch = Regex.Match(
            text,
            @"(?:Erwachsene|Damen|Herren|Jugend|Jungen|Mädchen|Senioren)[^()\n]{0,160}(?:Liga|Klasse|Bezirks|Kreis|Verbandsliga|Landesliga|Oberliga)[^()\n]{0,160}\([^)]+\)");

        if (detailedLeagueMatch.Success)
            return CleanInlineText(detailedLeagueMatch.Value);

        return text
            .Split('\n')
            .Select(CleanInlineText)
            .Where(IsLeagueLine)
            .OrderBy(line => line.Length)
            .FirstOrDefault();
    }

    private string? ExtractSeason(string text)
    {
        var seasonMatch = Regex.Match(text, @"\b20\d{2}\s*/\s*(?:20)?\d{2}\b");

        if (seasonMatch.Success)
            return seasonMatch.Value.Replace(" ", "");

        var shortSeasonMatch = Regex.Match(text, @"\b\d{2}\s*/\s*\d{2}\b");

        if (shortSeasonMatch.Success)
            return shortSeasonMatch.Value.Replace(" ", "");

        var yearMatch = Regex.Match(text, @"\b20\d{2}\b");

        return yearMatch.Success ? yearMatch.Value : null;
    }

    private bool IsLeagueLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || line.Length > 220)
            return false;

        var leagueKeywords = new[]
        {
            "Liga",
            "Klasse",
            "Bezirks",
            "Kreis",
            "Verbandsliga",
            "Landesliga",
            "Oberliga"
        };

        return leagueKeywords.Any(keyword =>
            line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private HtmlNode? FindBestTable(HtmlDocument doc, Func<HtmlNode, int> scoreSelector)
    {
        var tables = doc.DocumentNode.SelectNodes("//table");

        if (tables is null || tables.Count == 0)
            return null;

        return tables
            .Select(table => new
            {
                Table = table,
                Score = scoreSelector(table)
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .Select(result => result.Table)
            .FirstOrDefault();
    }

    private int GetScheduleTableScore(HtmlNode table)
    {
        var text = CleanInlineText(table.InnerText);
        var score = 0;

        foreach (var keyword in new[] { "Datum", "Zeit", "Heimmannschaft", "Gastmannschaft", "Spiele" })
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                score += 8;
        }

        if (score == 0)
            return 0;

        if (text.Contains("Bilanz", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Einsätze", StringComparison.OrdinalIgnoreCase))
        {
            score -= 20;
        }

        return score;
    }

    private int GetStandingsTableScore(HtmlNode table, HtmlNode? scheduleTable)
    {
        if (scheduleTable is not null && ReferenceEquals(table, scheduleTable))
            return 0;

        var text = CleanInlineText(table.InnerText);

        if (text.Contains("Heimmannschaft", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Gastmannschaft", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Datum", StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (!text.Contains("Mannschaft", StringComparison.OrdinalIgnoreCase))
            return 0;

        var score = 0;

        foreach (var keyword in new[] { "Begegnungen", "Punkte", "Spiele", "Sätze", "Platz", "Rang", "Tabellenplatz" })
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                score += 5;
        }

        if (score == 0)
            return 0;

        if (text.Contains("Spieler", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Bilanz", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("Einsätze", StringComparison.OrdinalIgnoreCase))
        {
            score -= 20;
        }

        var rowCount = table.SelectNodes(".//tr")?.Count ?? 0;
        score += Math.Min(rowCount, 15);

        return score;
    }

    private int GetStatisticsTableScore(HtmlNode table, HtmlNode? scheduleTable, HtmlNode? standingsTable)
    {
        if (scheduleTable is not null && ReferenceEquals(table, scheduleTable))
            return 0;

        if (standingsTable is not null && ReferenceEquals(table, standingsTable))
            return 0;

        var text = CleanInlineText(table.InnerText);
        var score = 0;

        foreach (var keyword in new[] { "Spieler", "Bilanz", "Einzel", "Doppel", "Einsätze", "Gesamt" })
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                score += 6;
        }

        if (score == 0)
            return 0;

        var rowCount = table.SelectNodes(".//tr")?.Count ?? 0;
        score += Math.Min(rowCount, 20);

        return score;
    }

    private List<List<string>> ReadTableRows(HtmlNode? table)
    {
        if (table is null)
            return new List<List<string>>();

        var rows = table.SelectNodes(".//tr");

        if (rows is null || rows.Count == 0)
            return new List<List<string>>();

        var result = new List<List<string>>();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes(".//th|.//td");

            if (cells is null)
                continue;

            var values = cells
                .Select(c => CleanInlineText(c.InnerText))
                .ToList();

            TrimTrailingEmptyCells(values);

            if (values.Any(value => !string.IsNullOrWhiteSpace(value)))
                result.Add(values);
        }

        return result;
    }

    private List<List<string>> ReadScheduleRows(HtmlNode? table)
    {
        var rows = ReadTableRows(table);

        if (rows.Count == 0)
            return new List<List<string>>();

        var result = new List<List<string>>
        {
            new() { "Datum", "Zeit", "Heim", "Gast", "Ergebnis" }
        };

        foreach (var row in rows.Skip(1))
        {
            var date = GetValue(row, 0);
            var time = ExtractTime(GetValue(row, 1));
            var resultIndex = row.FindLastIndex(IsResult);

            if (resultIndex < 3)
                continue;

            var gameResult = row[resultIndex];
            var guestTeam = row[resultIndex - 1];
            var homeTeam = row[resultIndex - 2];

            if (string.IsNullOrWhiteSpace(date) ||
                string.IsNullOrWhiteSpace(homeTeam) ||
                string.IsNullOrWhiteSpace(guestTeam))
            {
                continue;
            }

            result.Add(new List<string> { date, time, homeTeam, guestTeam, gameResult });
        }

        return result.Count == 1 ? new List<List<string>>() : result;
    }

    private string SerializeRows(List<List<string>> rows)
    {
        return JsonSerializer.Serialize(rows, JsonOptions);
    }

    private string GetValue(List<string> values, int index)
    {
        return index < values.Count ? values[index] : "";
    }

    private bool IsResult(string value)
    {
        return Regex.IsMatch(value, @"^\d+\s*:\s*\d+$");
    }

    private string ExtractTime(string value)
    {
        var match = Regex.Match(value, @"\d{1,2}:\d{2}");

        return match.Success ? match.Value : CleanInlineText(value);
    }

    private void TrimTrailingEmptyCells(List<string> values)
    {
        for (var index = values.Count - 1; index >= 0; index--)
        {
            if (!string.IsNullOrWhiteSpace(values[index]))
                break;

            values.RemoveAt(index);
        }
    }

    private string CleanMultilineText(string text)
    {
        return string.Join(
            "\n",
            HtmlEntity.DeEntitize(text)
                .Split('\n')
                .Select(CleanInlineText)
                .Where(line => !string.IsNullOrWhiteSpace(line)));
    }

    private string CleanInlineText(string text)
    {
        var decodedText = HtmlEntity.DeEntitize(text);

        return string.Join(
            " ",
            decodedText.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}

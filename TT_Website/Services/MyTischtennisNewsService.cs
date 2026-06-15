using HtmlAgilityPack;

namespace TT_Website.Services;

public record MyTischtennisNewsItem(
    string Title,
    string Url,
    string? Teaser,
    string? ImageUrl);

public class MyTischtennisNewsService
{
    private const string NewsOverviewUrl = "https://www.mytischtennis.de/news/alle-news";
    private static readonly Uri BaseUri = new("https://www.mytischtennis.de");
    private readonly HttpClient _httpClient;

    public MyTischtennisNewsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(12);
    }

    public async Task<IReadOnlyList<MyTischtennisNewsItem>> GetLatestAsync(int maxItems = 6)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, NewsOverviewUrl);
        request.Headers.UserAgent.ParseAdd("TT_Website/1.0");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return ExtractNewsItems(doc, maxItems);
    }

    private static IReadOnlyList<MyTischtennisNewsItem> ExtractNewsItems(HtmlDocument doc, int maxItems)
    {
        var anchors = doc.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>();
        var items = new List<MyTischtennisNewsItem>();
        var seenUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var anchor in anchors)
        {
            var href = anchor.GetAttributeValue("href", "");
            if (!LooksLikeNewsLink(href))
                continue;

            var url = ToAbsoluteUrl(href);
            if (url.EndsWith("/news/alle-news", StringComparison.OrdinalIgnoreCase) ||
                !seenUrls.Add(url))
            {
                continue;
            }

            var container = FindNewsContainer(anchor);
            var title = FindTitle(anchor);

            if (title.Length < 12 && container is not null)
                title = FindTitle(container);

            if (title.Length < 12)
                title = CleanText(anchor.InnerText);

            if (title.Length < 12)
                continue;

            var teaser = container is null ? null : FindTeaser(container, title);
            var imageUrl = container is null ? null : FindImageUrl(container);

            items.Add(new MyTischtennisNewsItem(title, url, teaser, imageUrl));

            if (items.Count >= maxItems)
                break;
        }

        return items;
    }

    private static bool LooksLikeNewsLink(string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return false;

        return href.Contains("/news/", StringComparison.OrdinalIgnoreCase) ||
            href.Contains("mytischtennis.de/news/", StringComparison.OrdinalIgnoreCase);
    }

    private static HtmlNode? FindNewsContainer(HtmlNode anchor)
    {
        return anchor
            .Ancestors()
            .Take(6)
            .FirstOrDefault(node =>
            {
                var name = node.Name.ToLowerInvariant();
                var className = node.GetAttributeValue("class", "");
                return name is "article" or "li" ||
                    className.Contains("news", StringComparison.OrdinalIgnoreCase) ||
                    className.Contains("article", StringComparison.OrdinalIgnoreCase) ||
                    className.Contains("teaser", StringComparison.OrdinalIgnoreCase);
            });
    }

    private static string FindTitle(HtmlNode container)
    {
        var heading = container
            .SelectNodes(".//h1|.//h2|.//h3|.//h4|.//a[@href]")
            ?.Select(node => CleanText(node.InnerText))
            .FirstOrDefault(text => text.Length >= 12);

        return heading ?? "";
    }

    private static string? FindTeaser(HtmlNode container, string title)
    {
        var teaser = container
            .SelectNodes(".//p|.//*[contains(@class,'teaser') or contains(@class,'intro') or contains(@class,'text')]")
            ?.Select(node => CleanText(node.InnerText))
            .FirstOrDefault(text =>
                text.Length >= 24 &&
                !text.Equals(title, StringComparison.OrdinalIgnoreCase));

        return string.IsNullOrWhiteSpace(teaser) ? null : teaser;
    }

    private static string? FindImageUrl(HtmlNode container)
    {
        var image = container.SelectSingleNode(".//img");
        var src = image?.GetAttributeValue("src", "") ?? "";

        if (string.IsNullOrWhiteSpace(src))
            src = image?.GetAttributeValue("data-src", "") ?? "";

        return string.IsNullOrWhiteSpace(src) ? null : ToAbsoluteUrl(src);
    }

    private static string ToAbsoluteUrl(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
            return absoluteUri.ToString();

        return new Uri(BaseUri, value).ToString();
    }

    private static string CleanText(string text)
    {
        return string.Join(
            " ",
            HtmlEntity.DeEntitize(text)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}

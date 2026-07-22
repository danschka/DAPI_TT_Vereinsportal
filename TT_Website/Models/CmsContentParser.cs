namespace TT_Website.Models;

public sealed record CmsContentBlock(string? Heading, string Text, string? ImagePath = null)
{
    public bool IsImage => !string.IsNullOrWhiteSpace(ImagePath);
}

public static class CmsContentParser
{
    public static IReadOnlyList<CmsContentBlock> Parse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return [];

        var lines = content.Replace("\r\n", "\n").Split('\n');

        if (!lines.Any(line => line.TrimStart().StartsWith("### ")
            || line.Trim().Equals(":::image", StringComparison.OrdinalIgnoreCase)))
        {
            return ParseLegacy(lines);
        }

        var blocks = new List<CmsContentBlock>();
        var heading = "";
        var textLines = new List<string>();

        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].TrimEnd();

            if (line.Trim().Equals(":::image", StringComparison.OrdinalIgnoreCase))
            {
                AddTextBlock(blocks, heading, textLines);
                heading = "";
                textLines.Clear();

                var imageLines = new List<string>();
                while (++index < lines.Length && !lines[index].Trim().Equals(":::", StringComparison.Ordinal))
                    imageLines.Add(lines[index].Trim());

                var imagePath = imageLines.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                var caption = string.Join("\n", imageLines.Skip(1).Where(x => !string.IsNullOrWhiteSpace(x)));

                if (!string.IsNullOrWhiteSpace(imagePath))
                    blocks.Add(new CmsContentBlock(null, caption, imagePath));

                continue;
            }

            if (line.TrimStart().StartsWith("### "))
            {
                AddTextBlock(blocks, heading, textLines);
                heading = line.Trim()[4..].Trim();
                textLines.Clear();
                continue;
            }

            textLines.Add(line);
        }

        AddTextBlock(blocks, heading, textLines);
        return blocks;
    }

    private static IReadOnlyList<CmsContentBlock> ParseLegacy(IEnumerable<string> rawLines)
    {
        var lines = rawLines.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var blocks = new List<CmsContentBlock>();
        var textBuffer = new List<string>();

        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];

            if (line.EndsWith('?') && index + 1 < lines.Count)
            {
                AddTextBlock(blocks, "", textBuffer);
                textBuffer.Clear();
                blocks.Add(new CmsContentBlock(line, lines[++index]));
                continue;
            }

            var questionMarkIndex = line.IndexOf('?');
            if (questionMarkIndex >= 0 && questionMarkIndex < line.Length - 1)
            {
                AddTextBlock(blocks, "", textBuffer);
                textBuffer.Clear();
                blocks.Add(new CmsContentBlock(
                    line[..(questionMarkIndex + 1)].Trim(),
                    line[(questionMarkIndex + 1)..].Trim()));
                continue;
            }

            textBuffer.Add(line);
        }

        AddTextBlock(blocks, "", textBuffer);
        return blocks;
    }

    private static void AddTextBlock(List<CmsContentBlock> blocks, string heading, List<string> lines)
    {
        var text = string.Join("\n", lines).Trim();
        if (!string.IsNullOrWhiteSpace(heading) || !string.IsNullOrWhiteSpace(text))
            blocks.Add(new CmsContentBlock(string.IsNullOrWhiteSpace(heading) ? null : heading, text));
    }
}

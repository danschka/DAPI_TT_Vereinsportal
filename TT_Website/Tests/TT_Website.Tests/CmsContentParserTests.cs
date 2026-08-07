using TT_Website.Models;

namespace TT_Website.Tests;

public class CmsContentParserTests
{
    [Fact]
    public void Parse_ReturnsNoBlocks_ForEmptyContent()
    {
        Assert.Empty(CmsContentParser.Parse("   "));
    }

    [Fact]
    public void Parse_SeparatesStructuredSections()
    {
        var blocks = CmsContentParser.Parse("### Training\nDienstag und Freitag\n\n### Ort\nSporthalle Bogen");

        Assert.Collection(blocks,
            first => { Assert.Equal("Training", first.Heading); Assert.Contains("Dienstag", first.Text); },
            second => { Assert.Equal("Ort", second.Heading); Assert.Equal("Sporthalle Bogen", second.Text); });
    }

    [Fact]
    public void Parse_RecognizesLegacyQuestionAndAnswer()
    {
        var block = Assert.Single(CmsContentParser.Parse("Wann trainieren wir? Dienstag und Freitag"));

        Assert.Equal("Wann trainieren wir?", block.Heading);
        Assert.Equal("Dienstag und Freitag", block.Text);
    }

    [Fact]
    public void Parse_RecognizesLegacyImageBlocks()
    {
        var block = Assert.Single(CmsContentParser.Parse(":::image\n/uploads/gallery/test.jpg\nBildtext\n:::"));

        Assert.True(block.IsImage);
        Assert.Equal("/uploads/gallery/test.jpg", block.ImagePath);
        Assert.Equal("Bildtext", block.Text);
    }
}

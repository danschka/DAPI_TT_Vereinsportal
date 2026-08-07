using TT_Website.Services;

namespace TT_Website.Tests;

public class ContentPageServiceTests
{
    [Theory]
    [InlineData(" Über Uns ", "ueber-uns")]
    [InlineData("Tischtennis TV", "tischtennis-tv")]
    [InlineData("Spaß & Sport", "spass-&-sport")]
    public void NormalizeSlug_ReturnsStableRoute(string input, string expected)
    {
        Assert.Equal(expected, ContentPageService.NormalizeSlug(input));
    }

    [Theory]
    [InlineData("single", "single")]
    [InlineData("image-left", "image-left")]
    [InlineData("unknown", "cards")]
    [InlineData(null, "cards")]
    public void NormalizeLayoutStyle_UsesKnownValuesOrFallback(string? input, string expected)
    {
        Assert.Equal(expected, ContentPageService.NormalizeLayoutStyle(input));
    }
}

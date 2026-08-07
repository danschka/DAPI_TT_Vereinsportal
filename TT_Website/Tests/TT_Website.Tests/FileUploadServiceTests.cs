using TT_Website.Services;

namespace TT_Website.Tests;

public class FileUploadServiceTests
{
    [Theory]
    [InlineData("foto.jpg")]
    [InlineData("FOTO.JPEG")]
    [InlineData("grafik.png")]
    [InlineData("animation.gif")]
    [InlineData("modern.webp")]
    public void IsAllowedImageFileName_AcceptsSupportedImages(string fileName)
    {
        Assert.True(FileUploadService.IsAllowedImageFileName(fileName));
    }

    [Theory]
    [InlineData("archiv.zip")]
    [InlineData("script.svg")]
    [InlineData("foto.jpg.exe")]
    [InlineData("")]
    public void IsAllowedImageFileName_RejectsUnsupportedFiles(string fileName)
    {
        Assert.False(FileUploadService.IsAllowedImageFileName(fileName));
    }
}

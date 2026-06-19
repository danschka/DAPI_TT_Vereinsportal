namespace TT_Website.Services;

public static class WebFilePathValidator
{
    public static string? GetExistingPath(IWebHostEnvironment environment, string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath) || !webPath.StartsWith('/'))
            return null;

        var webRoot = Path.GetFullPath(environment.WebRootPath);
        var relativePath = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath));

        if (!fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
            return null;

        return File.Exists(fullPath) ? webPath : null;
    }

    public static bool Exists(IWebHostEnvironment environment, string? webPath)
    {
        return GetExistingPath(environment, webPath) is not null;
    }
}

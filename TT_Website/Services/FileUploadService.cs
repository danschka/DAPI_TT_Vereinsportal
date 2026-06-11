using Microsoft.AspNetCore.Components.Forms;

namespace TT_Website.Services;

public enum UploadFileKind
{
    Image,
    Document
}

public class FileUploadService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".txt"
    };

    private static readonly HashSet<string> AllowedDocumentContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain"
    };

    private readonly IWebHostEnvironment _environment;

    public FileUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(
        IBrowserFile file,
        string uploadSubfolder,
        UploadFileKind kind,
        long maxAllowedSize)
    {
        ValidateFile(file, kind, maxAllowedSize);

        var safeFileName = CreateSafeFileName(file.Name);
        var folderPath = Path.Combine(_environment.WebRootPath, "uploads", uploadSubfolder);
        Directory.CreateDirectory(folderPath);

        var uploadPath = Path.Combine(folderPath, safeFileName);

        await using var fs = new FileStream(uploadPath, FileMode.CreateNew);
        await file.OpenReadStream(maxAllowedSize).CopyToAsync(fs);

        return $"/uploads/{uploadSubfolder}/{safeFileName}";
    }

    public void DeleteIfExists(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var normalizedPath = relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());

        if (!normalizedPath.StartsWith($"uploads{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            return;

        var filePath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, normalizedPath));
        var uploadsRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads"));

        if (!filePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            return;

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static void ValidateFile(IBrowserFile file, UploadFileKind kind, long maxAllowedSize)
    {
        if (file.Size <= 0)
            throw new InvalidOperationException("Die Datei ist leer.");

        if (file.Size > maxAllowedSize)
            throw new InvalidOperationException($"Die Datei ist zu groß. Maximal erlaubt sind {maxAllowedSize / 1024 / 1024} MB.");

        var extension = Path.GetExtension(file.Name);

        var validExtension = kind == UploadFileKind.Image
            ? AllowedImageExtensions.Contains(extension)
            : AllowedDocumentExtensions.Contains(extension);

        var validContentType = kind == UploadFileKind.Image
            ? AllowedImageContentTypes.Contains(file.ContentType)
            : AllowedDocumentContentTypes.Contains(file.ContentType);

        if (!validExtension || !validContentType)
        {
            var allowed = kind == UploadFileKind.Image
                ? "JPG, PNG oder WEBP"
                : "PDF, Word, Excel oder TXT";

            throw new InvalidOperationException($"Dieser Dateityp ist nicht erlaubt. Erlaubt sind: {allowed}.");
        }
    }

    private static string CreateSafeFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var safeBaseName = new string(baseName
            .Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_')
            .Take(40)
            .ToArray());

        if (string.IsNullOrWhiteSpace(safeBaseName))
        {
            safeBaseName = "datei";
        }

        return $"{Guid.NewGuid():N}_{safeBaseName}{extension}";
    }
}

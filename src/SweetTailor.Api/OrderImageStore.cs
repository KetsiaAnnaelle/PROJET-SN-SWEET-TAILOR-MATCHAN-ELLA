namespace SweetTailor.Api;

public sealed class OrderImageStore
{
    public const int MaxImageBytes = 5 * 1024 * 1024;

    private readonly string _directoryPath;

    public OrderImageStore(IHostEnvironment environment)
    {
        _directoryPath = Path.Combine(environment.ContentRootPath, "App_Data", "OrderImages");
    }

    public async Task<string> SaveAsync(Stream source, CancellationToken cancellationToken)
    {
        await using var content = new MemoryStream();
        var buffer = new byte[81920];
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            if (content.Length + bytesRead > MaxImageBytes)
            {
                throw new InvalidDataException("La photo ne peut pas dépasser 5 Mo.");
            }

            await content.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }

        var extension = GetImageExtension(content.GetBuffer().AsSpan(0, (int)content.Length));
        var fileName = $"{Guid.NewGuid():N}{extension}";
        Directory.CreateDirectory(_directoryPath);
        await File.WriteAllBytesAsync(
            Path.Combine(_directoryPath, fileName),
            content.ToArray(),
            cancellationToken);

        return fileName;
    }

    public FileStream? OpenRead(string fileName)
    {
        if (!IsStoredImageName(fileName))
        {
            return null;
        }

        var path = Path.Combine(_directoryPath, fileName);
        return File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete)
            : null;
    }

    public void Delete(string? fileName)
    {
        if (fileName is null || !IsStoredImageName(fileName))
        {
            return;
        }

        var path = Path.Combine(_directoryPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static bool IsStoredImageName(string? fileName)
    {
        if (fileName is null ||
            string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal) ||
            fileName.Contains('/') ||
            fileName.Contains('\\'))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _) &&
               extension is ".jpg" or ".png" or ".webp";
    }

    public static string GetContentType(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => throw new InvalidDataException("Format d’image non pris en charge.")
        };

    private static string GetImageExtension(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            return ".jpg";
        }

        ReadOnlySpan<byte> pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (header.Length >= pngSignature.Length && header[..pngSignature.Length].SequenceEqual(pngSignature))
        {
            return ".png";
        }

        if (header.Length >= 12 &&
            header[..4].SequenceEqual("RIFF"u8) &&
            header.Slice(8, 4).SequenceEqual("WEBP"u8))
        {
            return ".webp";
        }

        throw new InvalidDataException("La photo doit être au format JPEG, PNG ou WebP.");
    }
}

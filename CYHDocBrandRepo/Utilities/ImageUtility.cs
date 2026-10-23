using System.IO;

namespace CYHDocBrandRepo.Utilities;

public class ImageProcessResult
{
    public bool Success { get; set; }
    public string? RelativePath { get; set; }
    public string? FileName { get; set; }
    public long FinalSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
}

public static class ImageUtility
{
    // Maximum allowed file size: Strictly 1 MB (1,048,576 bytes)
    public const long MaxImageSizeBytes = 1024 * 1024; // 1 MB

    // Allowed image file extensions
    public static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>
    /// Validates whether the given file extension is a supported image format.
    /// </summary>
    public static bool IsValidImageExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }

    /// <summary>
    /// Checks if a given byte size is strictly within the 1 MB ceiling.
    /// </summary>
    public static bool IsWithin1Mb(long bytes) => bytes is > 0 and <= MaxImageSizeBytes;

    /// <summary>
    /// Human-readable file size formatter.
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{(bytes / 1024.0):F1} KB";
        return $"{(bytes / (1024.0 * 1024.0)):F2} MB";
    }

    /// <summary>
    /// Validates image binary header (Magic Bytes) to ensure genuine image content.
    /// Prevents renamed malware or corrupted files.
    /// </summary>
    public static bool HasValidImageHeader(byte[] buffer)
    {
        if (buffer == null || buffer.Length < 4) return false;

        // JPEG: FF D8 FF
        if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF) return true;

        // PNG: 89 50 4E 47
        if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47) return true;

        // WebP: RIFF ... WEBP
        if (buffer.Length >= 12 &&
            buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46 &&
            buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50) return true;

        return false;
    }

    /// <summary>
    /// Processes and writes the image stream to disk while guaranteeing that the file size
    /// does NOT exceed 1 MB (1,048,576 bytes).
    /// </summary>
    public static async Task<ImageProcessResult> SaveImageWithinLimitAsync(
        Stream inputStream,
        string originalFileName,
        string targetDirectory,
        string urlPrefix,
        string fileIdentifier)
    {
        if (!IsValidImageExtension(originalFileName))
        {
            return new ImageProcessResult
            {
                Success = false,
                ErrorMessage = "Invalid image format. Allowed formats: JPG, JPEG, PNG, WEBP."
            };
        }

        try
        {
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

            // Read into buffer to inspect size and headers
            using var memoryStream = new MemoryStream();
            var buffer = new byte[81920];
            int read;
            long totalBytes = 0;

            while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                totalBytes += read;
                if (totalBytes > MaxImageSizeBytes)
                {
                    return new ImageProcessResult
                    {
                        Success = false,
                        ErrorMessage = $"Image size exceeds the 1 MB limit (Size: {FormatFileSize(totalBytes)}). Image size must not exceed 1 MB."
                    };
                }
                memoryStream.Write(buffer, 0, read);
            }

            if (totalBytes == 0)
            {
                return new ImageProcessResult
                {
                    Success = false,
                    ErrorMessage = "Uploaded image file is empty."
                };
            }

            // Verify binary header
            var imageBytes = memoryStream.ToArray();
            if (!HasValidImageHeader(imageBytes))
            {
                return new ImageProcessResult
                {
                    Success = false,
                    ErrorMessage = "The uploaded file is not a valid image or is corrupted."
                };
            }

            // Generate clean unique filename
            var fileName = $"doc_{fileIdentifier}_{DateTime.UtcNow.Ticks}{extension}";
            var fullPath = Path.Combine(targetDirectory, fileName);

            // Write to physical disk
            await File.WriteAllBytesAsync(fullPath, imageBytes);

            var relativeUrl = $"{urlPrefix.TrimEnd('/')}/{fileName}";

            return new ImageProcessResult
            {
                Success = true,
                FileName = fileName,
                RelativePath = relativeUrl,
                FinalSizeBytes = totalBytes
            };
        }
        catch (Exception ex)
        {
            return new ImageProcessResult
            {
                Success = false,
                ErrorMessage = $"Error saving image: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Safely deletes an existing physical image file from the disk.
    /// </summary>
    public static void DeletePhysicalImage(string? webRootPath, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(webRootPath) || string.IsNullOrWhiteSpace(relativePath)) return;

        try
        {
            var cleanRelative = relativePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRootPath, cleanRelative);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch
        {
            // Suppress error if file is already deleted or in use
        }
    }
}

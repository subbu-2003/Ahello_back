using NReco.VideoInfo; // Install: NReco.VideoInfo NuGet package

public class VideoValidationService
{
    private const long MaxSizeBytes = 10 * 1024 * 1024; // 10 MB
    private const double MaxDurationSeconds = 30.0;
    private static readonly string[] AllowedTypes = { "video/mp4", "video/webm", "video/quicktime" };

    public (bool isValid, string error) Validate(IFormFile file)
    {
        // 1. Check size
        if (file.Length > MaxSizeBytes)
            return (false, "Video must be under 10MB.");

        // 2. Check MIME type
        if (!AllowedTypes.Contains(file.ContentType.ToLower()))
            return (false, "Only MP4, WebM, or MOV videos are allowed.");

        // 3. Check duration using NReco.VideoInfo
        var tempPath = Path.GetTempFileName() + Path.GetExtension(file.FileName);
        try
        {
            using (var stream = new FileStream(tempPath, FileMode.Create))
                file.CopyTo(stream);

            var probe = new FFProbe();
            var info = probe.GetMediaInfo(tempPath);

            if (info.Duration.TotalSeconds > MaxDurationSeconds)
                return (false, $"Video must be 30 seconds or less (yours: {info.Duration.TotalSeconds:F1}s).");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }

        return (true, string.Empty);
    }
}
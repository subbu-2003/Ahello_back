public class VideoValidationService
{
    private const long MaxSizeBytes = 10 * 1024 * 1024; // 10 MB
    private static readonly string[] AllowedTypes = { "video/mp4", "video/webm", "video/quicktime" };
    private static readonly string[] AllowedExtensions = { ".mp4", ".webm", ".mov" };

    public (bool isValid, string error) Validate(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, "No video file provided.");

        if (file.Length > MaxSizeBytes)
            return (false, "Video must be under 10MB.");

        if (string.IsNullOrEmpty(file.ContentType) ||
            !AllowedTypes.Contains(file.ContentType.ToLower()))
            return (false, "Only MP4, WebM, or MOV videos are allowed.");

        var ext = Path.GetExtension(file.FileName)?.ToLower();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return (false, "Invalid video file extension.");

        return (true, string.Empty);
    }
}
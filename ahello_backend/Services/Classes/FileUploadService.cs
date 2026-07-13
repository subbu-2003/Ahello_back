public class FileUploadService
{
    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveVideoAsync(IFormFile file)
    {
        var folder = Path.Combine(_env.WebRootPath, "uploads", "videos");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(folder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/videos/{fileName}"; // returns relative URL
    }

    public void DeleteVideo(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var fullPath = Path.Combine(_env.WebRootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
    public async Task<string> SaveFileAsync(IFormFile file, string folderName = "forms")
    {
        var root = _env.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var folder = Path.Combine(root, "uploads", folderName);

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(folder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folderName}/{fileName}";
    }

    public void DeleteFile(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        var root = _env.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var fullPath = Path.Combine(root, relativePath.TrimStart('/'));

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }
    public async Task<string> SaveChatFileAsync(IFormFile file)
    {
        const long maxFileSize = 10 * 1024 * 1024; // 10 MB

        if (file.Length > maxFileSize)
            throw new Exception("File size cannot exceed 10 MB.");

        var allowedExtensions = new[]
        {
        // Images
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg",

        // Documents
        ".pdf", ".doc", ".docx",
        ".xls", ".xlsx",
        ".ppt", ".pptx",
        ".txt", ".csv",

        // Archives
        //".zip", ".rar", ".7z"
    };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new Exception("File type is not supported.");

        var root = _env.WebRootPath
            ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        var folder = Path.Combine(root, "uploads", "chat");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/chat/{fileName}";
    }
}

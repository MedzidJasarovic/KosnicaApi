namespace KosnicaApi.Services;

public class MockStorageService : IStorageService
{
    private readonly ILogger<MockStorageService> _logger;

    public MockStorageService(ILogger<MockStorageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        _logger.LogInformation($"[MOCK] Uploading file {file.FileName} to {folderName}...");
        
        // Simulate network delay
        await Task.Delay(500);

        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        var fakeId = Guid.NewGuid().ToString().Substring(0, 8);
        
        // Return a mock Cloudinary URL
        return $"https://res.cloudinary.com/demo/image/upload/{folderName}/{fakeId}{fileExtension}";
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        _logger.LogInformation($"[MOCK] Deleting file at {fileUrl}...");
        await Task.Delay(200);
        return true;
    }
}

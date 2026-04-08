namespace KosnicaApi.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folderName);
    Task<bool> DeleteFileAsync(string fileUrl);
}

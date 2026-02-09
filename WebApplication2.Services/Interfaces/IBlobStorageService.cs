using Microsoft.AspNetCore.Http;

namespace WebApplication2.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadFile(IFormFile formFile,  string blobName, string containerName);
        Task DeleteFile(string blobName, string containerName);
    }
}

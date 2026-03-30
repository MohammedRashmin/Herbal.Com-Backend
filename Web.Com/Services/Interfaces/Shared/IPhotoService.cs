using CloudinaryDotNet.Actions;

namespace Web.Com.Services.Interfaces.Shared;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}

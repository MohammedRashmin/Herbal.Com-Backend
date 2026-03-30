using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Com.Services.Interfaces.Shared;

namespace Web.Com.Controllers.Shared;

[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _photoService;

    public PhotosController(IPhotoService photoService)
    {
        _photoService = photoService;
    }

    /// <summary>
    /// Upload image to Cloudinary (500x500 crop fill). Returns SecureUrl and PublicId.
    /// Frontend sends file as multipart/form-data with key "file".
    /// </summary>
    [HttpPost("add-photo")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddPhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null)
            return BadRequest(result.Error.Message);

        return Ok(new
        {
            url = result.SecureUrl.AbsoluteUri,
            publicId = result.PublicId
        });
    }
}

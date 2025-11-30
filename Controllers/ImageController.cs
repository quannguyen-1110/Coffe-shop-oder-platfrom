using CoffeeShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ImageController : ControllerBase
    {
        private readonly IS3Service _s3Service;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public ImageController(IS3Service s3Service, IConfiguration configuration)
        {
            _s3Service = s3Service;
            _configuration = configuration;
            _bucketName = _configuration["AWS:S3:BucketName"] ?? "coffeeshop-product-images ";
        }

        [HttpPost("upload/{type}")]
        public async Task<IActionResult> UploadImage(IFormFile file, string type)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest("Invalid file type. Only images are allowed.");

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size exceeds 5MB limit");

            try
            {
                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var fileName = $"{type}/{Guid.NewGuid()}{fileExtension}";

                var imageUrl = await _s3Service.UploadFileAsync(file, _bucketName, fileName);

                return Ok(new { 
                    message = "File uploaded successfully", 
                    imageUrl = imageUrl,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading file: {ex.Message}");
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteImage([FromQuery] string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("File name is required");

            try
            {
                var success = await _s3Service.DeleteFileAsync(_bucketName, fileName);
                
                if (success)
                    return Ok(new { message = "File deleted successfully" });
                else
                    return NotFound("File not found or could not be deleted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting file: {ex.Message}");
            }
        }
    }
}
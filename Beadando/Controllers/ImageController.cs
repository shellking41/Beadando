using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Beadando.Services;

namespace Beadando.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly SessionService _sessionService;

        public ImageController(IWebHostEnvironment webHostEnvironment, SessionService sessionService)
        {
            _webHostEnvironment = webHostEnvironment;
            _sessionService = sessionService;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            try
            {
                await _sessionService.ValidateSessionAsync(Request, Response);

                if (image == null || image.Length == 0)
                    return BadRequest("No image file provided");

                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                string imagePath = $"/uploads/{fileName}";

                return Ok(new
                {
                    message = "Image uploaded successfully",
                    imagePath = imagePath
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace StudentApi.Controllers
{
    [Authorize]
    [Route("api/images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UploadFile(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            string uploadedDirectory = @"D:\MyUpload";

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(uploadedDirectory, fileName);

            if (!Directory.Exists(uploadedDirectory))
            {
                Directory.CreateDirectory(uploadedDirectory);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return Ok(new { filePath });
        }


        [HttpGet("{fileName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetImage(string fileName)
        {
            string uploadedDirectory = @"D:\MyUpload";
            string filePath = Path.Combine(uploadedDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Image not found!");
            }

            FileStream imageStream = System.IO.File.OpenRead(filePath);
            string mimeType = GetMimeType(filePath);

            return File(imageStream, mimeType);

        }

        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}

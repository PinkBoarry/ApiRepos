using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Web.Controllers
{
    [ApiController]
    [Route("")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile uploadedFile)
        {
            return StatusCode(200, new JsonResult(await UploadFile(uploadedFile)) );
        }

        [HttpPost("uploadWithRedirect")]
        public async Task<IActionResult> UploadWithReditrect(IFormFile uploadedFile)
        {
            return Redirect(await UploadFile(uploadedFile));
        }

        [HttpGet("images/{imageName}")]
        public IActionResult GetImage(string imageName)
        {
            var imageBytes = System.IO.File.ReadAllBytes($"wwwroot\\{imageName}");
            return File(imageBytes, "image/png");
        }

        private async Task<string> UploadFile(IFormFile uploadedFile)
        {
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4()?.ToString() ?? String.Empty;
            _logger.LogInformation($"[FileController] Uploading file ${uploadedFile.FileName} from ${ipAddress}");


            if (uploadedFile != null)
            {
                string hash = Convert.ToHexString(MD5.Create().ComputeHash(Guid.NewGuid().ToByteArray()));
                string extension = Path.GetExtension(uploadedFile.FileName);
                string newFileName = hash + extension;

                using (var fileStream = new FileStream(Path.Combine("wwwroot/", newFileName), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                _logger.LogInformation($"[FileController] File ${uploadedFile.FileName} saved as {newFileName}");
                return newFileName;
            }
            _logger.LogWarning($"[FileController] Error upload file ${uploadedFile?.FileName} (not uploaded)");
            return String.Empty;
        }
    }
}
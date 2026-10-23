
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MotorInsuranceAPI.Services
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 5 * 1024 * 1024;

        public ImageStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveClaimImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new ArgumentException("Please upload an image.");
            }

            if (image.Length > MaxFileSize)
            {
                throw new ArgumentException(
                    "Image size cannot exceed 5 MB."
                );
            }

            var extension = Path.GetExtension(image.FileName)
                .ToLowerInvariant();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    "Only JPG, JPEG, PNG and WEBP images are allowed."
                );
            }

            if (!await HasValidImageSignatureAsync(image, extension))
            {
                throw new ArgumentException(
                    "The uploaded file is not a valid supported image."
                );
            }

            var uploadFolder = Path.Combine(
                _environment.ContentRootPath,
                "wwwroot",
                "uploads",
                "claims"
            );

            Directory.CreateDirectory(uploadFolder);

            // Generate a unique filename instead of trusting the original name.
            var fileName = $"{Guid.NewGuid():N}{extension}";

            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write))
            {
                await image.CopyToAsync(stream);
            }

            return $"/uploads/claims/{fileName}";
        }

        private static async Task<bool> HasValidImageSignatureAsync(
            IFormFile image,
            string extension)
        {
            var header = new byte[12];

            using var stream = image.OpenReadStream();

            var bytesRead = await stream.ReadAsync(
                header.AsMemory(0, header.Length)
            );

            if (extension == ".jpg" || extension == ".jpeg")
            {
                return bytesRead >= 3
                    && header[0] == 0xFF
                    && header[1] == 0xD8
                    && header[2] == 0xFF;
            }

            if (extension == ".png")
            {
                byte[] pngSignature =
                {
                    0x89, 0x50, 0x4E, 0x47,
                    0x0D, 0x0A, 0x1A, 0x0A
                };

                return bytesRead >= 8
                    && header.Take(8).SequenceEqual(pngSignature);
            }

            if (extension == ".webp")
            {
                return bytesRead >= 12
                    && header[0] == 'R'
                    && header[1] == 'I'
                    && header[2] == 'F'
                    && header[3] == 'F'
                    && header[8] == 'W'
                    && header[9] == 'E'
                    && header[10] == 'B'
                    && header[11] == 'P';
            }

            return false;
        }
    }
}

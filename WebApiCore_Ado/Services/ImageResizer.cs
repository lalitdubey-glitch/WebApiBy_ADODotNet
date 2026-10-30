using Microsoft.AspNetCore.Http.HttpResults;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WebApiCore_Ado.Controllers;

namespace WebApiCore_Ado.Services
{
    public class ImageResizer : IImageResizer
    {
        private readonly ILogger<UserController> _logger;
        public ImageResizer(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        public async Task<string> ResizeImage(IFormFile? file, string? filePath = null, int maxHeight = 800, int maxWidth = 800, int quality = 70)
        {
            if (file == null || file.Length <= 0)
            {
                return null;
            }

            string[] allowExt = { ".jpeg", ".jpg", ".png" };
            string[] AllowedTypes = { "image/jpeg", "image/png", "image/jpeg" };

            string FileExt = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowExt.Contains(FileExt))
            {
                _logger.LogError("File Type Not Supported , Only JPEG, JPG, PNG File type supported");
                return "fileNotSupported";
            }

            Directory.CreateDirectory(filePath);

            var uniqueName = $"{Guid.NewGuid()}.jpg";
            var fullPath = Path.Combine(filePath, uniqueName);

            using var stream = file.OpenReadStream();

            using var Img =await Image.LoadAsync(stream); 

            Img.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));

            var encoder = new JpegEncoder { Quality = quality };
            await Img.SaveAsJpegAsync(fullPath, encoder);

            return uniqueName;


        }
    }
}

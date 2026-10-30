namespace WebApiCore_Ado.Services
{
    public interface IImageResizer
    {
        Task<string> ResizeImage(IFormFile? file, string? filePath = null, int maxHeight = 800, int maxWidth = 800, int quality = 70);
    }
}
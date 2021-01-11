using Microsoft.AspNetCore.Http;

namespace Matbox.BLL.DTO
{
    public class FilesDto
    {
        public IFormFile uploadedFile { get; set; }
        public string category { get; set; }
    }
}
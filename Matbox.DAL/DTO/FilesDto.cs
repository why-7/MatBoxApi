using Microsoft.AspNetCore.Http;

namespace Matbox.DAL.DTO
{
    public class FilesDto
    {
        public IFormFile uploadedFile { get; set; }
        public string category { get; set; }
    }
}
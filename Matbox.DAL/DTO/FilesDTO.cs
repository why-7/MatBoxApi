using Microsoft.AspNetCore.Http;

namespace Matbox.DAL.DTO
{
    public class FilesDTO
    {
        public IFormFile uploadedFile { get; set; }
        public string category { get; set; }
    }
}
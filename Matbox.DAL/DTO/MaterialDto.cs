using System;
using Microsoft.AspNetCore.Http;

namespace Matbox.DAL.DTO
{
    public class MaterialDto
    {
        public IFormFile uploadedFile { get; set; }
        public string materialName { get; set; }
        public string category { get; set; }
        public int versionNumber { get; set; }
    }
}
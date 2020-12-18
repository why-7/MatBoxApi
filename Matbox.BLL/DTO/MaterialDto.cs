using System;
using Microsoft.AspNetCore.Http;

namespace Matbox.BLL.DTO
{
    public class MaterialDto
    {
        public int id { get; set; }
        public IFormFile uploadedFile { get; set; }
        public string materialName { get; set; }
        public string category { get; set; }
        public int versionNumber { get; set; }
        public long minSize { get; set; } 
        public long maxSize { get; set; }
        
        public string path { get; set; }
        public DateTime metaDateTime { get; set; }
        public double metaFileSize { get; set; }
    }
}
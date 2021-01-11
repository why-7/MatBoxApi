using System;

namespace Matbox.BLL.DTO
{
    public class MaterialDto
    {
        public string materialName { get; set; }
        public string category { get; set; }
        public int versionNumber { get; set; }
        public string path { get; set; }
        public DateTime metaDateTime { get; set; }
        public double metaFileSize { get; set; }
    }
}
using System;

namespace Matbox.WEB.Dto
{
    public class MaterialDto
    { 
        public string materialName { get; set; }
        public string category { get; set; }
        public int versionNumber { get; set; }
        public DateTime metaDateTime { get; set; }
        public double metaFileSize { get; set; }
    }
}
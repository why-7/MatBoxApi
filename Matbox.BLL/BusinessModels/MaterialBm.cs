using System;

namespace Matbox.BLL.BusinessModels
{
    public class MaterialBm
    {
        public byte[] fileBytes { get; set; }
        public string materialName { get; set; }
        public string category { get; set; }
        public int versionNumber { get; set; }
        public DateTime metaDateTime { get; set; }
        public double metaFileSize { get; set; }
        public string userId { get; set; }
    }
}
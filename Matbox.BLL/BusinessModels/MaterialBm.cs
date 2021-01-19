using System;

namespace Matbox.BLL.BusinessModels
{
    public class MaterialBm
    {
        public byte[] FileBytes { get; set; }
        public string MaterialName { get; set; }
        public string Category { get; set; }
        public int VersionNumber { get; set; }
        public DateTime MetaDateTime { get; set; }
        public double MetaFileSize { get; set; }
        public string UserId { get; set; }
    }
}
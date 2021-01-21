using System;

namespace Matbox.BLL.BusinessModels
{
    public class MaterialBm
    {
        public byte[] FileBytes { get; set; }
        public string MaterialName { get; set; }
        public int Category { get; set; }
        public int VersionNumber { get; set; }
        public string UserId { get; set; }
    }
}
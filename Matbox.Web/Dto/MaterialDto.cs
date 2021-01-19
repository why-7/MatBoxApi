using System;

namespace Matbox.Web.Dto
{
    public class MaterialDto
    { 
        public string MaterialName { get; set; }
        public int Category { get; set; }
        public int VersionNumber { get; set; }
        public DateTime MetaDateTime { get; set; }
        public double MetaFileSize { get; set; }
    }
}
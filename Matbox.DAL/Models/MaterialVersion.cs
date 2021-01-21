using System;

namespace Matbox.DAL.Models
{
    public class MaterialVersion
    {
        public int Id { get; set; }
        public string Hash { get; set;  }
        public int VersionNumber { get; set; }
        public DateTime MetaDateTime { get; set; }
        public double MetaFileSize { get; set; }
        public Material Material { get; set; }
        public int MaterialId { get; set; }
    }
}
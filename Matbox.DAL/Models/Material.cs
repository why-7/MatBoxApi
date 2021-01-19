using System;

namespace Matbox.DAL.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public int Category { get; set; }
        public string Hash { get; set;  }
        public int VersionNumber { get; set; }
        public DateTime MetaDateTime { get; set; }
        public double MetaFileSize { get; set; }
        public string UserId { get; set; }
    }
}
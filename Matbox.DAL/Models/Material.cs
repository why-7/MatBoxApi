using System;

namespace Matbox.DAL.Models
{
    public class Material
    {
        public int id { get; set; }
        public string materialName { get; set; }
        public string path { get; set; }
        public string category { get; set; }
        public string hash { get; set;  }
        public int versionNumber { get; set; }
        public DateTime metaDateTime { get; set; }
        public double metaFileSize { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace MatBoxApi.Models
{
    public class Material
    {
        public int id { get; set; }
        public string materialName { get; set; }
        public string path { get; set; }
        public string category { get; set; }
        public int versionNumber { get; set; }
        public DateTime metaDateTime { get; set; }
        public double metaFileSize { get; set; }
    }
}
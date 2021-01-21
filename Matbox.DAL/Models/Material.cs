using System;
using System.Collections.Generic;

namespace Matbox.DAL.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public int Category { get; set; }
        public string UserId { get; set; }
        public ICollection<MaterialVersion> Versions { get; set; }
    }
}